using Issue.Models;
using Issue.Models.Dtos;

namespace Issue.Interfaces;

public interface IIssueService
{
    Task<IEnumerable<IssueEntry>> GetAllIssuesAsync();
    Task<IssueEntry> CreateIssueAsync(IssueEntry issue);
    Task<bool> DeleteIssueAsync(Guid id, string ownerId);
    Task<IssueEntry> EditIssueAsync(Guid id, string userId, IssuePatchRequest issuePatch);
}
