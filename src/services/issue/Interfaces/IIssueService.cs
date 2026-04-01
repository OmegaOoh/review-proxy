using Issue.Models;

namespace Issue.Interfaces;

public interface IIssueService
{
    Task<IEnumerable<IssueEntry>> GetAllIssuesAsync();
    Task<IssueEntry> CreateIssueAsync(IssueEntry issue);
    Task<bool> DeleteIssueAsync(Guid id, string ownerId);
}
