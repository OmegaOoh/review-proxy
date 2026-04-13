using Repository.Data;
using Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repository.Services;

public class AuditorQueryService(
    RepoDbContext dbContext,
    IIdentityClient identityClient) : IAuditorQueryService
{
    public async Task<List<Guid>> GetAuditorsAsync(Guid repoId)
    {
        var repo = await dbContext.Repositories.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == repoId);

        if (repo == null) return [];

        var auditors = repo.AuditorsIds.Select(Guid.Parse).ToList();
        if (Guid.TryParse(repo.OwnerId, out var ownerId) && !auditors.Contains(ownerId))
        {
            auditors.Add(ownerId);
        }

        return auditors;
    }

    public async Task<List<object>> GetAuditorsDetailsAsync(Guid repoId, string? authorizationHeader)
    {
        var ids = await GetAuditorsAsync(repoId);
        if (ids.Count == 0) return [];

        return await identityClient.GetUsersBatchAsync(ids, authorizationHeader);
    }
}
