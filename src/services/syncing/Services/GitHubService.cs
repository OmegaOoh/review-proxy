using GitHub;
using GitHub.Models;
using ReviewProxy.Contracts;
using Syncing.Interfaces;
using Syncing.Models;

namespace Syncing.Services;

public class GitHubService(
    IConfiguration configuration,
    ILogger<GitHubService> logger,
    IGitHubClientFactory clientFactory,
    IIdentityClient identityClient) : IGitHubService
{
    public async Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken)
    {
        var github = clientFactory.CreateGitHubClient(accessToken);
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

        var jwt = clientFactory.GenerateGitHubJwt(appId, await File.ReadAllTextAsync(keyPath));
        var github = clientFactory.CreateGitHubClient(jwt);

        var owner = approvalEvent.GitHubRepoId.Split('/')[0];
        var installations = await github.App.Installations.GetAsync();

        var inst = installations?.FirstOrDefault(i =>
            i.Account?.SimpleUser?.Login?.Equals(owner, StringComparison.OrdinalIgnoreCase) == true);

        if (inst?.Id == null) throw new InvalidOperationException($"No installation found for owner {owner}");

        var tokenResponse = await github.App.Installations[(int)inst.Id.Value].Access_tokens.PostAsync(new GitHub.App.Installations.Item.Access_tokens.Access_tokensPostRequestBody());
        if (string.IsNullOrEmpty(tokenResponse?.Token)) throw new InvalidOperationException("Token creation failed");

        var instClient = clientFactory.CreateGitHubClient(tokenResponse.Token);
        var repoParts = approvalEvent.GitHubRepoId.Split('/');
        if (repoParts.Length < 2) throw new InvalidOperationException($"Invalid GitHubRepoId: {approvalEvent.GitHubRepoId}");
        var repoOwner = repoParts[0];
        var repoName = repoParts[1];

        var creator = await identityClient.GetGitHubUserByIdAsync(approvalEvent.IssueOwner);

        var creatorMention = creator != null ? $"@{creator.GitHubUsername}" : "Unknown User";

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

    public async Task<object> GetSyncContextAsync()
    {
        var appSlug = configuration["GitHub:AppSlug"] ?? "review-proxy";
        return new
        {
            app_slug = appSlug,
            installation_url = $"https://github.com/apps/{appSlug}/installations/new"
        };
    }
}
