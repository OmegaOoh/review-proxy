using Repository.Interfaces;

namespace Repository.Services;

public class AuditorService : IAuditorService
{
    public Task AddAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        return Task.CompletedTask;
    }

    public Task<List<Guid>> GetAuditorsAsync(Guid repoId)
    {
        return Task.FromResult(new List<Guid>());
    }

    public Task RemoveAuditorsAsync(Guid repoId, List<Guid> userId)
    {
        return Task.CompletedTask;
    }
}
