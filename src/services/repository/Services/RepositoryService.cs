using Repository.Data;
using Repository.Interfaces;
using Repository.Models;

namespace Repository.Services;

public class RepositoryService(
    RepoDbContext dbContext,
    IRepositoryEventPublisher eventPublisher,
    IGitHubRepositoryService githubService) : IRepositoryService
{
    public async Task<bool> DeleteRepositoryAsync(Guid repoId, string ownerId)
    {
        var entry = await dbContext.Repositories.FindAsync(repoId);
        if (entry == null) return false;

        // Treat "not owner" the same as "not found" to avoid leaking repository existence
        if (entry.OwnerId != ownerId) return false;

        dbContext.Repositories.Remove(entry);
        await dbContext.SaveChangesAsync();
        await eventPublisher.PublishRepositoryDeletedAsync(repoId);
        return true;
    }

    public async Task<Guid> DepositAsync(string githubRepoId, string ownerId, string description, string githubToken, List<Guid> auditors)
    {
        var (fullName, _) = await githubService.ValidateRepositoryAsync(githubRepoId, githubToken);

        var entry = new RepositoryEntry
        {
            Id = Guid.NewGuid(),
            GitHubRepoId = fullName,
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
