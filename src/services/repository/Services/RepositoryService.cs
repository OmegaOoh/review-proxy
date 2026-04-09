using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Services;

public class RepositoryService(RepoDbContext dbContext, IHttpClientFactory httpClientFactory, IRepositoryEventPublisher eventPublisher) : IRepositoryService
{
    public async Task<bool> DeleteRepositoryAsync(Guid repoId, string ownerId)
    {
        var entry = await dbContext.Repositories.FindAsync(repoId);
        if (entry == null) return false;

        // Treat "not owner" the same as "not found" to avoid leaking repository existence
        if (entry.OwnerId != ownerId) return false;

        dbContext.Repositories.Remove(entry);
        await dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<Guid> DepositAsync(string githubRepoId, string ownerId, string ownerUsername, string description, List<Guid> auditors)
    {
        if (string.IsNullOrWhiteSpace(githubRepoId) ||
            !githubRepoId.StartsWith(ownerUsername + "/", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Repository must be owned by the authenticated user.");
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");
        var response = await client.GetAsync($"https://api.github.com/repos/{githubRepoId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Invalid or inaccessible GitHub repository.");
        }

        var entry = new RepositoryEntry
        {
            Id = Guid.NewGuid(),
            GitHubRepoId = githubRepoId,
            OwnerId = ownerId,
            Description = description,
            AuditorsIds = auditors.Select(a => a.ToString()).Where(a => a != ownerId).Distinct().ToList(),
            CreatedAt = DateTime.UtcNow
        };


        dbContext.Repositories.Add(entry);
        await dbContext.SaveChangesAsync();
        await eventPublisher.PublishAuditorListAsync(entry.Id);
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
