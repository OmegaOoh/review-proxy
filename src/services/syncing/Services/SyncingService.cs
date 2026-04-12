using Syncing.Interfaces;
using System.Net.Http.Json;
using Octokit;
using Microsoft.Extensions.Configuration;
using ReviewProxy.Contracts;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Syncing.Services;

public class SyncingService : ISyncingService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<SyncingService> _logger;

    public SyncingService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<SyncingService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;

        var appId = _configuration["GitHub:AppId"];
        var keyPath = _configuration["GitHub:PrivateKeyPath"];
        _logger.LogInformation("SyncingService initialized. AppId: {AppId}, PrivateKeyPath: {KeyPath}",
                appId ?? "NOT SET", keyPath ?? "NOT SET");
    }

    public async Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl)
    {
        var client = _httpClientFactory.CreateClient("identity");

        var response = await client.PostAsJsonAsync("/api/identities/exchange", new
        {
            GitHubId = githubId,
            Username = username,
            AvatarUrl = avatarUrl
        });

        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            user = result.GetProperty("user"),
            token = result.GetProperty("token").GetString()
        });
    }

    public async Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken)
    {
        var github = new GitHubClient(new ProductHeaderValue("ReviewProxy"))
        {
            Credentials = new Credentials(accessToken)
        };

        try
        {
            var installationsResponse = await github.GitHubApps.GetAllInstallationsForCurrentUser();
            var allRepos = new List<Repository>();

            foreach (var installation in installationsResponse.Installations)
            {
                var reposResponse = await github.GitHubApps.Installation.GetAllRepositoriesForCurrentUser(installation.Id);
                allRepos.AddRange(reposResponse.Repositories);
            }

            if (allRepos.Any())
            {
                return allRepos.DistinctBy(r => r.Id).Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    full_name = r.FullName,
                    description = r.Description,
                    html_url = r.HtmlUrl,
                    @private = r.Private
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching via installations");
        }
        var userRepos = await github.Repository.GetAllForCurrent();
        return userRepos.Select(r => new
        {
            id = r.Id,
            name = r.Name,
            full_name = r.FullName,
            description = r.Description,
            html_url = r.HtmlUrl,
            @private = r.Private
        });
    }

    public async Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent)
    {
        var appId = _configuration["GitHub:AppId"];
        var keyPath = _configuration["GitHub:PrivateKeyPath"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(keyPath))
        {
            throw new InvalidOperationException("GitHub App credentials (AppId or PrivateKeyPath) are not configured.");
        }

        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException($"GitHub Private Key file not found at {keyPath}");
        }

        var rawKey = await File.ReadAllTextAsync(keyPath);
        var jwt = GenerateGitHubJwt(appId, rawKey);

        var github = new GitHubClient(new ProductHeaderValue("ReviewProxy"))
        {
            Credentials = new Credentials(jwt, AuthenticationType.Bearer)
        };

        if (!long.TryParse(approvalEvent.GitHubRepoId, out var repoId))
        {
            throw new ArgumentException("Invalid GitHub Repository ID.");
        }

        var installation = await github.GitHubApps.GetRepositoryInstallationForCurrent(repoId);
        var response = await github.GitHubApps.CreateInstallationToken(installation.Id);

        var installationClient = new GitHubClient(new ProductHeaderValue("ReviewProxy"))
        {
            Credentials = new Credentials(response.Token)
        };

        var newIssue = new NewIssue(approvalEvent.Title)
        {
            Body = $"{approvalEvent.Body}\n\n---\nReviewProxy Issue ID: {approvalEvent.IssueId}\nApproved by user {approvalEvent.ApproverId} via ReviewProxy at {approvalEvent.UtcTime}."
        };

        await installationClient.Issue.Create(repoId, newIssue);
    }

    private string GenerateGitHubJwt(string appId, string privateKeyPem)
    {
        using var rsa = RSA.Create();

        // Even when reading from file, we handle potential escaped \n just in case,
        // though a standard .pem file should be fine.
        var pem = privateKeyPem.Replace("\\n", "\n").Trim();

        try
        {
            rsa.ImportFromPem(pem.ToCharArray());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to import RSA key from PEM. Ensure the file at PrivateKeyPath is a valid PEM file.");
            throw;
        }

        var handler = new JwtSecurityTokenHandler();
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = appId,
            IssuedAt = DateTime.UtcNow.AddSeconds(-60),
            Expires = DateTime.UtcNow.AddMinutes(9),
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        };

        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
