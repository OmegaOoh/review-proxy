namespace Repository.Interfaces;

public interface IAuditorService
{
    Task AddAuditorsAsync(Guid repoId, List<Guid> userId);
    Task RemoveAuditorsAsync(Guid repoId, List<Guid> userId);
    Task<List<Guid>> GetAuditorsAsync(Guid repoId);
    Task<List<object>> GetAuditorsDetailsAsync(Guid repoId, string? authorizationHeader);
}
