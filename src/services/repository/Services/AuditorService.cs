using Repository.Data;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository.Services;

public class AuditorService(RepoDbContext dbContext) : IAuditorService
{
    public async Task AddAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return;

        foreach (var id in userId)
        {
            var idStr = id.ToString();
            // Do not add owner to auditors list
            if (repo.OwnerId != idStr && !repo.AuditorsIds.Contains(idStr))
            {
                repo.AuditorsIds.Add(idStr);
            }
        }

        await dbContext.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetAuditorsAsync(Guid repoId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return [];

        var auditors = repo.AuditorsIds.Select(Guid.Parse).ToList();
        if (Guid.TryParse(repo.OwnerId, out var ownerId) && !auditors.Contains(ownerId))
        {
            auditors.Add(ownerId);
        }

        return auditors;
    }

    public async Task RemoveAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return;

        foreach (var id in userId)
        {
            var idStr = id.ToString();
            // Owner is not eligible to remove themselves
            if (repo.OwnerId == idStr) continue;

            repo.AuditorsIds.Remove(idStr);
        }

        await dbContext.SaveChangesAsync();
    }
}
