namespace Repository.Interfaces.Services;

public interface IAuditorService
{
    Task AddAuditorsAsync(Guid repoId, List<Guid> userId);
    Task RemoveAuditorsAsync(Guid repoId, List<Guid> userId);
}
