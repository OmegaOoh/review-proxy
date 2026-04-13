using Repository.Models;

namespace Repository.Interfaces;

public interface IRepositoryQueryService
{
    Task<List<RepositoryEntry>> GetRepositoriesAsync(Guid? ownerId = null, RepositoryRole? role = null);
}
