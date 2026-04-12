using ReviewProxy.Contracts;

namespace Syncing.Interfaces;

public interface IGitHubService
{
    Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken);
    Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent);
}
