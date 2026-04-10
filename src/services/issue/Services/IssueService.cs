using Microsoft.EntityFrameworkCore;
using Issue.Data;
using Issue.Interfaces;
using Issue.Models;
using Issue.Models.Dtos;

namespace Issue.Services;

public class IssueService(IssueDbContext context) : IIssueService
{
    private readonly IssueDbContext _context = context;

    public async Task<IEnumerable<IssueEntry>> GetAllIssuesAsync()
    {
        return await _context.Issues.ToListAsync();
    }

    public async Task<IssueEntry> CreateIssueAsync(IssueEntry issue)
    {
        var repo = await _context.Repositories.FindAsync(issue.RepositoryId);
        if (repo == null)
            throw new ArgumentException("Repository not found.");

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
        bool isAuditorOrOwner = repo != null && Guid.TryParse(ownerId, out var userGuid) && repo.AuditorsId.Contains(userGuid);

        // Allow the owner of the issue, or an auditor/repo owner to delete the issue
        if (issue.OwnerId != ownerId && !isAuditorOrOwner) return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IssueEntry> EditIssueAsync(Guid id, string userId, IssuePatchRequest issuePatch)
    {
        var existingIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == id && i.OwnerId == userId);


        if (existingIssue == null) throw new KeyNotFoundException("Issue not found or you do not have permission to edit it.");

        // Disallow editing of approved issues
        if (existingIssue.Status == IssueStatus.Approved) throw new InvalidOperationException("Cannot edit an approved issue.");

        IssueStatus[] whiteListStatus = [IssueStatus.Draft, IssueStatus.SubmitForReview];

        if (issuePatch.Title != null) existingIssue.Title = issuePatch.Title;
        if (issuePatch.Body != null) existingIssue.Body = issuePatch.Body;

        if (issuePatch.Status.HasValue && whiteListStatus.Contains(issuePatch.Status.Value))
        {
            existingIssue.Status = issuePatch.Status.Value;
        }

        // Set status to draft if rejected and edited (while not explicitly stated in this patch)
        if (existingIssue.Status == IssueStatus.Rejected && !issuePatch.Status.HasValue)
        {
            existingIssue.Status = IssueStatus.Draft;
        }

        existingIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return existingIssue;
    }

    public async Task<bool> ApproveIssueAsync(Guid id, string userId)
    {
        var existingIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException($"Issue with id {id} not found");
        if (existingIssue.Status != IssueStatus.SubmitForReview) throw new InvalidOperationException($"Issue with id {id} is not submitted for review");
        if (existingIssue.OwnerId == userId) throw new InvalidOperationException($"Issue with id {id} cannot be approved by the owner");

        var repo = await _context.Repositories.FindAsync(existingIssue.RepositoryId) ?? throw new KeyNotFoundException($"Cannot find repository with id {existingIssue.RepositoryId}");

        // Findout if the user is an auditor of the repository
        if (!Guid.TryParse(userId, out var userGuid) || !repo.AuditorsId.Contains(userGuid)) throw new UnauthorizedAccessException($"User {userId} is not an auditor of repository {existingIssue.RepositoryId}");


        existingIssue.Status = IssueStatus.Approved;
        existingIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RejectIssueAsync(Guid id, string userId)
    {
        var existingIssue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == id) ?? throw new KeyNotFoundException($"Issue with id {id} not found");
        if (existingIssue.Status != IssueStatus.SubmitForReview) throw new InvalidOperationException($"Issue with id {id} is not submitted for review");
        if (existingIssue.OwnerId == userId) throw new InvalidOperationException($"Issue with id {id} cannot be approved by the owner");

        var repo = await _context.Repositories.FindAsync(existingIssue.RepositoryId) ?? throw new KeyNotFoundException($"Cannot find repository with id {existingIssue.RepositoryId}");

        // Findout if the user is an auditor of the repository
        if (!Guid.TryParse(userId, out var userGuid) || !repo.AuditorsId.Contains(userGuid)) throw new UnauthorizedAccessException($"User {userId} is not an auditor of repository {existingIssue.RepositoryId}");


        existingIssue.Status = IssueStatus.Rejected;
        existingIssue.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }
}
