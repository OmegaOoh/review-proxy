using Microsoft.EntityFrameworkCore;
using Issue.Data;
using Issue.Interfaces;
using Issue.Models;

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
}
