using ReviewProxy.Contracts;
using Issue.Models;

namespace Issue.Services;

public partial class IssueService
{
    public async Task<bool> ApproveIssueAsync(Guid id, string userId)
    {
        var (issue, repo, userGuid) = await ValidateReviewRequest(id, userId);

        issue.Status = IssueStatus.Approved;
        issue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        await _publishEndpoint.Publish(new IssueApprovalEvent
        {
            IssueId = issue.Id,
            ApproverId = userGuid,
            GitHubRepoId = repo.GitHubRepoId,
            Title = issue.Title,
            Body = issue.Body,
            IssueOwner = issue.OwnerId,
            UtcTime = issue.UpdatedAt
        });

        return true;
    }

    public async Task<bool> RejectIssueAsync(Guid id, string userId)
    {
        var (issue, _, _) = await ValidateReviewRequest(id, userId);

        issue.Status = IssueStatus.Rejected;
        issue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
