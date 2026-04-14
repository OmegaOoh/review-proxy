namespace Repository.Interfaces.Services;

using Repository.Models;


public interface IRepositoryQueryService
{
    Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null);
}
