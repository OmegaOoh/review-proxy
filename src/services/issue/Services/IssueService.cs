using Microsoft.EntityFrameworkCore;
using Issue.Data;
using Issue.Interfaces;
using Issue.Models;

namespace Issue.Services;

public class IssueService : IIssueService
{
    private readonly IssueDbContext _context;

    public IssueService(IssueDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<IssueEntry>> GetAllIssuesAsync()
    {
        return await _context.Issues.ToListAsync();
    }

    public async Task<IssueEntry> CreateIssueAsync(IssueEntry issue)
    {
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

        // Only allow the owner to delete the issue
        if (issue.OwnerId != ownerId) return false;

        _context.Issues.Remove(issue);
        await _context.SaveChangesAsync();

        return true;
    }
}
