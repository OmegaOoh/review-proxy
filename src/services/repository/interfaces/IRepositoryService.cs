using Repository.Models;

namespace Repository.Interfaces;

public interface IRepositoryService
{
    Task<Guid> DepositAsync(string githubRepoId, string ownerId, string description);
    Task UpdateRepositoryAsync(Guid repoId, string description);
    Task DeleteRepositoryAsync(Guid repoId);
    Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null);
}
