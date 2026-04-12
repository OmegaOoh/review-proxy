using System.Security.Claims;
using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using GitHub.Models;
using Microsoft.Kiota.Abstractions.Authentication;
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

        try { await FetchAllInstallationsReposAsync(github, allRepos); }
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
            @private = r.Private
        });
    }

    private async Task FetchAllInstallationsReposAsync(GitHubClient github, List<Repository> allRepos)
    {
        int page = 1, perPage = 100;
        while (true)
        {
            var response = await github.User.Installations.GetAsync(p => { p.QueryParameters.Page = page; p.QueryParameters.PerPage = perPage; });
            if (response?.Installations == null || !response.Installations.Any()) break;

            foreach (var inst in response.Installations)
                await FetchInstallationReposAsync(github, inst.Id ?? 0, allRepos);

            if (response.Installations.Count < perPage) break;
            page++;
        }
    }

    private async Task FetchInstallationReposAsync(GitHubClient github, long installationId, List<Repository> allRepos)
    {
        int page = 1, perPage = 100;
        while (true)
        {
            var response = await github.User.Installations[(int)installationId].Repositories.GetAsync(p => { p.QueryParameters.Page = page; p.QueryParameters.PerPage = perPage; });
            if (response?.Repositories == null || !response.Repositories.Any()) break;

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
        var repo = await github.Repos[""][""].WithUrl($"https://api.github.com/repos/{approvalEvent.GitHubRepoId}").GetAsync();
        if (repo?.Owner?.Login == null || repo.Name == null) throw new InvalidOperationException("Repo details not found");

        await instClient.Repos[repo.Owner.Login][repo.Name].Issues.PostAsync(new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody
        {
            Title = new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody.IssuesPostRequestBody_title { String = approvalEvent.Title },
            Body = $"{approvalEvent.Body}\n\n---\nReviewProxy Issue ID: {approvalEvent.IssueId}\nApproved by user {approvalEvent.ApproverId} via ReviewProxy at {approvalEvent.UtcTime}.\n"
        });
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
