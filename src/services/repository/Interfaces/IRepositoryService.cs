using Repository.Models;

namespace Repository.Interfaces;

public interface IRepositoryService
{
    Task<Guid> DepositAsync(string githubRepoId, string ownerId, string ownerUsername, string description, List<Guid> auditors);
    Task UpdateRepositoryAsync(Guid repoId, string description);
    Task<bool> DeleteRepositoryAsync(Guid repoId, string ownerId);
    Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null);
    Task PublishAuditorListAsync(Guid repoId);
}
