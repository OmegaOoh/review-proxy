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
    private const int DefaultPageSize = 100;
    private const int StartPage = 1;
    private const int ExpectedRepoIdParts = 2;

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
        int page = StartPage;
        while (true)
        {
            var response = await github.User.Repos.GetAsync(p =>
            {
                p.QueryParameters.Page = page;
                p.QueryParameters.PerPage = DefaultPageSize;
            });

            if (response == null || !response.Any()) break;

            allRepos.AddRange(response);
            if (response.Count < DefaultPageSize) break;
            page++;
        }
    }

    public async Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent)
    {
        var owner = approvalEvent.GitHubRepoId.Split('/')[0];
        var token = await installationService.GetInstallationTokenAsync(owner);
        var instClient = clientFactory.CreateGitHubClient(token);

        var repoParts = approvalEvent.GitHubRepoId.Split('/');
        if (repoParts.Length < ExpectedRepoIdParts)
            throw new InvalidOperationException($"Invalid GitHubRepoId: {approvalEvent.GitHubRepoId}");

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

        var issueRequest = new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody
        {
            Title = new GitHub.Repos.Item.Item.Issues.IssuesPostRequestBody.IssuesPostRequestBody_title
            {
                String = approvalEvent.Title
            },
            Body = issueBody
        };

        await instClient.Repos[repoOwner][repoName].Issues.PostAsync(issueRequest);
    }

    public Task<object> GetSyncContextAsync() => installationService.GetSyncContextAsync();
}
