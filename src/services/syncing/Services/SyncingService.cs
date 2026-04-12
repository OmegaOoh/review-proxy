using Syncing.Interfaces;
using System.Net.Http.Json;
using Octokit;
using GitHubJwt;
using Microsoft.Extensions.Configuration;
using ReviewProxy.Contracts;

namespace Syncing.Services;

public class SyncingService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ISyncingService
{
    private readonly IConfiguration _configuration = configuration;

    public async Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl)
    {
        var client = httpClientFactory.CreateClient("identity");

        var response = await client.PostAsJsonAsync("/api/identities/exchange", new
        {
            GitHubId = githubId,
            Username = username,
            AvatarUrl = avatarUrl
        });

        response.EnsureSuccessStatusCode();

        // The identity service returns { User: ..., Token: ... }
        // We parse and re-emit as camelCase { user: ..., token: ... } for the frontend.
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
            // 1. Get user installations for this GitHub App
            // API: GET /user/installations
            var installationsResponse = await github.GitHubApps.GetAllInstallationsForCurrentUser();

            var allRepos = new List<Repository>();

            foreach (var installation in installationsResponse.Installations)
            {
                // 2. For each installation, get repositories accessible to this user
                // API: GET /user/installations/{installation_id}/repositories
                var reposResponse = await github.GitHubApps.Installation.GetAllRepositoriesForCurrentUser(installation.Id);
                allRepos.AddRange(reposResponse.Repositories);
            }

            // If we found repos via installations, return them.
            // This is more efficient because it's scoped to what the App can see.
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
            // Log error or fall back
            Console.WriteLine($"Error fetching via installations: {ex.Message}");
        }
        // Fallback: List all repositories the user has access to directly
        // This might be less efficient if the user has thousands of repos,
        // but it works even if they haven't installed the app yet.
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
        var privateKey = _configuration["GitHub:PrivateKey"]?.Replace("\\n", "\n");

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(privateKey))
        {
            throw new InvalidOperationException("GitHub App credentials are not configured.");
        }

        var generator = new GitHubJwtFactory(
            new StringPrivateKeySource(privateKey),
            new GitHubJwtFactoryOptions
            {
                AppIntegrationId = int.Parse(appId),
                ExpirationSeconds = 600 // 10 minutes
            }
        );

        var jwt = generator.CreateEncodedJwtToken();

        var github = new GitHubClient(new ProductHeaderValue("ReviewProxy"))
        {
            Credentials = new Credentials(jwt, AuthenticationType.Bearer)
        };

        if (!long.TryParse(approvalEvent.GitHubRepoId, out var repoId))
        {
            throw new ArgumentException("Invalid GitHub Repository ID.");
        }

        // Find the installation for this repository
        var installation = await github.GitHubApps.GetRepositoryInstallationForCurrent(repoId);

        // Get an installation token
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
}
