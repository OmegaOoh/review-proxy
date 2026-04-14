namespace Repository.Interfaces.Services;

public interface IRepositoryService
{
    Task<Guid> DepositAsync(string githubRepoId, string ownerId, string description, string githubToken, List<Guid> auditors);
    Task UpdateRepositoryAsync(Guid repoId, string description);
    Task<bool> DeleteRepositoryAsync(Guid repoId, string ownerId);
}
