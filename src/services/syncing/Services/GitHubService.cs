using GitHub;
using GitHub.Models;
using ReviewProxy.Contracts;
using Syncing.Interfaces;
using Syncing.Models;

namespace Syncing.Services;

public class GitHubService(
    ILogger<GitHubService> logger,
    IGitHubClientFactory clientFactory,
    IIdentityClient identityClient,
    IGitHubInstallationService installationService) : IGitHubService
{
    public async Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken)
    {
        var github = clientFactory.CreateGitHubClient(accessToken);
        var allRepos = new List<Repository>();
        var installedRepoIds = new HashSet<long>();

        try
        {
            var (instRepos, instIds) = await installationService.GetUserInstallationReposAsync(accessToken);
            allRepos.AddRange(instRepos);
            foreach (var id in instIds) installedRepoIds.Add(id);
        }
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
        var owner = approvalEvent.GitHubRepoId.Split('/')[0];
        var token = await installationService.GetInstallationTokenAsync(owner);
        var instClient = clientFactory.CreateGitHubClient(token);

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

    public Task<object> GetSyncContextAsync() => installationService.GetSyncContextAsync();
}
