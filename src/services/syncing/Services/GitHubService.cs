using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using Microsoft.IdentityModel.Tokens;
using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using GitHub.Models;
using ReviewProxy.Contracts;
using Syncing.Interfaces;

namespace Syncing.Services;

public class GitHubService(IConfiguration configuration, ILogger<GitHubService> logger, IHttpClientFactory httpClientFactory) : IGitHubService
{
    private GitHubClient CreateGitHubClient(string token)
    {
        var httpClient = httpClientFactory.CreateClient("github");
        var tokenProvider = new TokenProvider(token);
        var adapter = RequestAdapter.Create(new TokenAuthProvider(tokenProvider), httpClient);
        return new GitHubClient(adapter);
    }

    public async Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken)
    {
        var github = CreateGitHubClient(accessToken);
        var allRepos = new List<Repository>();
        var installedRepoIds = new HashSet<long>();

        try { await FetchAllInstallationsReposAsync(github, allRepos, installedRepoIds); }
        catch (Exception ex) { logger.LogError(ex, "Error fetching via installations"); }

        try { await FetchDirectUserReposAsync(github, allRepos); }
        catch (Exception ex) { logger.LogError(ex, "Error fetching direct user repos"); }

        return allRepos.DistinctBy(r => r.Id).Select(r => new
        {
            id = r.Id,
            name = r.Name,
            full_name = r.FullName,
            description = r.Description,
            html_url = r.HtmlUrl,
            @private = r.Private,
            is_installed = installedRepoIds.Contains(r.Id ?? 0)
        });
    }

    private async Task FetchAllInstallationsReposAsync(GitHubClient github, List<Repository> allRepos, HashSet<long> installedRepoIds)
    {
        int page = 1, perPage = 100;
        while (true)
        {
            var response = await github.User.Installations.GetAsync(p => { p.QueryParameters.Page = page; p.QueryParameters.PerPage = perPage; });
            if (response?.Installations == null || !response.Installations.Any()) break;

            foreach (var inst in response.Installations)
                await FetchInstallationReposAsync(github, inst.Id ?? 0, allRepos, installedRepoIds);

            if (response.Installations.Count < perPage) break;
            page++;
        }
    }

    private async Task FetchInstallationReposAsync(GitHubClient github, long installationId, List<Repository> allRepos, HashSet<long> installedRepoIds)
    {
        int page = 1, perPage = 100;
        while (true)
        {
            var response = await github.User.Installations[(int)installationId].Repositories.GetAsync(p => { p.QueryParameters.Page = page; p.QueryParameters.PerPage = perPage; });
            if (response?.Repositories == null || !response.Repositories.Any()) break;

            foreach (var repo in response.Repositories)
            {
                if (repo.Id.HasValue) installedRepoIds.Add(repo.Id.Value);
            }
            allRepos.AddRange(response.Repositories);

            if (response.Repositories.Count < perPage) break;
            page++;
        }
    }

    private async Task FetchDirectUserReposAsync(GitHubClient github, List<Repository> allRepos)
    {
        int page = 1, perPage = 100;
        while (true)
        {
            var response = await github.User.Repos.GetAsync(p => { p.QueryParameters.Page = page; p.QueryParameters.PerPage = perPage; });
            if (response == null || !response.Any()) break;

            allRepos.AddRange(response);
            if (response.Count < perPage) break;
            page++;
        }
    }

    public async Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent)
    {
        var appId = configuration["GitHub:AppId"] ?? throw new InvalidOperationException("AppId not set");
        var keyPath = configuration["GitHub:PrivateKeyPath"] ?? throw new InvalidOperationException("KeyPath not set");
        logger.LogInformation("Syncing to GitHub: {Repo}, AppId: {AppId}", approvalEvent.GitHubRepoId, appId);

        var jwt = GenerateGitHubJwt(appId, await File.ReadAllTextAsync(keyPath));
        var github = CreateGitHubClient(jwt);

        var owner = approvalEvent.GitHubRepoId.Split('/')[0];
        var installations = await github.App.Installations.GetAsync();

        var inst = installations?.FirstOrDefault(i =>
            i.Account?.SimpleUser?.Login?.Equals(owner, StringComparison.OrdinalIgnoreCase) == true);

        if (inst?.Id == null) throw new InvalidOperationException($"No installation found for owner {owner}");

        var tokenResponse = await github.App.Installations[(int)inst.Id.Value].Access_tokens.PostAsync(new GitHub.App.Installations.Item.Access_tokens.Access_tokensPostRequestBody());
        if (string.IsNullOrEmpty(tokenResponse?.Token)) throw new InvalidOperationException("Token creation failed");

        var instClient = CreateGitHubClient(tokenResponse.Token);
        var repoParts = approvalEvent.GitHubRepoId.Split('/');
        if (repoParts.Length < 2) throw new InvalidOperationException($"Invalid GitHubRepoId: {approvalEvent.GitHubRepoId}");
        var repoOwner = repoParts[0];
        var repoName = repoParts[1];

        // Fetch user details from Identity Service
        // Both IssueOwner and ApproverId are stored as internal Guids in their respective services
        var creator = await GetGitHubUserByIdAsync(approvalEvent.IssueOwner);
        var approver = await GetGitHubUserByIdAsync(approvalEvent.ApproverId.ToString());

        var creatorMention = creator != null ? $"@{creator.GitHubUsername}" : "Unknown User";
        var approverGitHubId = approver?.GitHubID ?? "N/A";

        var issueBody = $"""
            {approvalEvent.Body}

            ---
            **Reported by:** {creatorMention}

            <sub>**ReviewProxy Metadata**
            Issue ID: `{approvalEvent.IssueId}`
            Approver ID: `{approvalEvent.ApproverId}`
            Approved at: {approvalEvent.UtcTime:yyyy-MM-dd HH:mm:ss} UTC</sub>
            """;

        await instClient.Repos[repoOwner][repoName].Issues.PostAsync(new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody
        {
            Title = new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody.IssuesPostRequestBody_title { String = approvalEvent.Title },
            Body = issueBody
        });
    }

    private async Task<GitHubUser?> GetGitHubUserByIdAsync(string userId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("identity");
            return await client.GetFromJsonAsync<GitHubUser>($"/api/identities/{userId}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch user {UserId} by internal ID from identity service", userId);
            return null;
        }
    }

    private async Task<GitHubUser?> GetGitHubUserByGitHubIdAsync(string githubId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("identity");
            return await client.GetFromJsonAsync<GitHubUser>($"/api/identities/github/{githubId}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch user {GitHubId} by GitHub ID from identity service", githubId);
            return null;
        }
    }

    private record GitHubUser(string GitHubUsername, string GitHubID);

    public async Task<object> GetSyncContextAsync()
    {
        var appSlug = configuration["GitHub:AppSlug"] ?? "review-proxy";
        return new
        {
            app_slug = appSlug,
            installation_url = $"https://github.com/apps/{appSlug}/installations/new"
        };
    }

    private string GenerateGitHubJwt(string appId, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        try { rsa.ImportFromPem(privateKeyPem.Replace("\\n", "\n").Trim().ToCharArray()); }
        catch (Exception ex) { logger.LogError(ex, "RSA import failed. Check PrivateKeyPath."); throw; }

        var handler = new JwtSecurityTokenHandler();
        var now = DateTimeOffset.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = appId,
            IssuedAt = now.AddSeconds(-60).UtcDateTime,
            Expires = now.AddMinutes(9).UtcDateTime,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
