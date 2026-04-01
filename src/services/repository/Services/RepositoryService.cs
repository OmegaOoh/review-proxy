using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Services;

public class RepositoryService(RepoDbContext dbContext) : IRepositoryService
{
    public async Task DeleteRepositoryAsync(Guid repoId)
    {
        var entry = await dbContext.Repositories.FindAsync(repoId);
        if (entry != null)
        {
            dbContext.Repositories.Remove(entry);
            await dbContext.SaveChangesAsync();
        }
    }

    public async Task<Guid> DepositAsync(string githubRepoId, string ownerId, string description)
    {
        var entry = new RepositoryEntry
        {
            Id = Guid.NewGuid(),
            GitHubRepoId = githubRepoId,
            OwnerId = ownerId,
            Description = description,
            CreatedAt = DateTime.UtcNow
        };

        dbContext.Repositories.Add(entry);
        await dbContext.SaveChangesAsync();

        return entry.Id;
    }

    public async Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null)
    {
        var query = dbContext.Repositories.AsQueryable();

        if (ownerId.HasValue)
        {
            var ownerIdStr = ownerId.Value.ToString();

            if (role.HasValue)
            {
                if (role.Value == RepositoryRole.Owner)
                {
                    query = query.Where(r => r.OwnerId == ownerIdStr);
                }
                else if (role.Value == RepositoryRole.Auditor)
                {
                    query = query.Where(r => r.AuditorsIds.Contains(ownerIdStr));
                }
            }
            else
            {
                query = query.Where(r => r.OwnerId == ownerIdStr || r.AuditorsIds.Contains(ownerIdStr));
            }
        }

        return await query.ToListAsync();
    }

    public async Task UpdateRepositoryAsync(Guid repoId, string description)
    {
        var entry = await dbContext.Repositories.FindAsync(repoId);
        if (entry != null)
        {
            entry.Description = description;
            await dbContext.SaveChangesAsync();
        }
    }
}
