using Microsoft.EntityFrameworkCore;
using Issue.Models;
using Issue.Models.Dtos;

namespace Issue.Services;

public partial class IssueService
{
    public async Task<IEnumerable<IssueEntry>> GetAllIssuesAsync()
    {
        return await _context.Issues.ToListAsync();
    }

    public async Task<IssueEntry> CreateIssueAsync(IssueEntry issue)
    {
        await GetRepositoryOrThrowAsync(issue.RepositoryId);

        issue.Id = Guid.NewGuid();
        issue.CreatedAt = DateTime.UtcNow;
        issue.UpdatedAt = DateTime.UtcNow;

        _context.Issues.Add(issue);
        await _context.SaveChangesAsync();

        return issue;
    }

    public async Task<bool> DeleteIssueAsync(Guid id, string ownerId)
    {
        var issue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == id);
        if (issue == null) return false;

        var repo = await _context.Repositories.FindAsync(issue.RepositoryId);
        bool isAuditor = repo != null && Guid.TryParse(ownerId, out var userGuid) && repo.AuditorsId.Contains(userGuid);

        // Allow the owner of the issue, or an auditor/repo owner to delete the issue
        if (issue.OwnerId != ownerId && !isAuditor) return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IssueEntry> EditIssueAsync(Guid id, string userId, IssuePatchRequest issuePatch)
    {
        var existingIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == id && i.OwnerId == userId)
                            ?? throw new KeyNotFoundException("Issue not found or you do not have permission to edit it.");

        // Disallow editing of approved issues
        if (existingIssue.Status == IssueStatus.Approved)
            throw new InvalidOperationException("Cannot edit an approved issue.");

        if (issuePatch.Title != null) existingIssue.Title = issuePatch.Title;
        if (issuePatch.Body != null) existingIssue.Body = issuePatch.Body;

        IssueStatus[] allowStatusChange = [IssueStatus.Draft, IssueStatus.SubmitForReview];
        if (issuePatch.Status.HasValue && allowStatusChange.Contains(issuePatch.Status.Value))
        {
            existingIssue.Status = issuePatch.Status.Value;
        }

        // Set status to draft if rejected and edited
        if (existingIssue.Status == IssueStatus.Rejected && !issuePatch.Status.HasValue)
        {
            existingIssue.Status = IssueStatus.Draft;
        }

        existingIssue.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return existingIssue;
    }
}
