using Repository.Data;
using Repository.Interfaces;

namespace Repository.Services;

public class AuditorService(
    RepoDbContext dbContext,
    IRepositoryEventPublisher eventPublisher) : IAuditorService
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

        var saved = await dbContext.SaveChangesAsync();
        if (saved > 0)
        {
            await eventPublisher.PublishAuditorListAsync(repoId);
        }
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

        var saved = await dbContext.SaveChangesAsync();
        if (saved > 0)
        {
            await eventPublisher.PublishAuditorListAsync(repoId);
        }
    }
}
