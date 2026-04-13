using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Services;

public class RepositoryQueryService(RepoDbContext dbContext) : IRepositoryQueryService
{
    public async Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null)
    {
        var query = dbContext.Repositories.AsQueryable();

        if (ownerId.HasValue)
        {
            var ownerIdStr = ownerId.Value.ToString();

            if (role.HasValue)
            {
                if (role.Value == RepositoryRole.Owner)
                    query = query.Where(r => r.OwnerId == ownerIdStr);
                else if (role.Value == RepositoryRole.Auditor)
                    query = query.Where(r => r.AuditorsIds.Contains(ownerIdStr));
            }
            else
            {
                query = query.Where(r => r.OwnerId == ownerIdStr || r.AuditorsIds.Contains(ownerIdStr));
            }
        }

        return await query.ToListAsync();
    }
}
