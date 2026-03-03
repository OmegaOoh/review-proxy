using Repository.Interfaces;
using Repository.Models;

namespace Repository.Services;

public class RepositoryService : IRepositoryService
{
    public Task DeleteRepositoryAsync(Guid repoId)
    {
        return Task.CompletedTask;
    }

    public Task<Guid> DepositAsync(string githubRepoId, string ownerId, string description)
    {
        return Task.FromResult(new Guid());
    }

    public Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null)
    {
        return Task.FromResult(new List<RepositoryEntry>());
    }

    public Task UpdateRepositoryAsync(Guid repoId, string description)
    {
        return Task.FromResult(Results.Ok());
    }
}
