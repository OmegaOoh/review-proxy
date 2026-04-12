using ReviewProxy.Contracts;

namespace Syncing.Interfaces;

public interface ISyncingService
{
    Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl);
    Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken);
    Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent);
}
