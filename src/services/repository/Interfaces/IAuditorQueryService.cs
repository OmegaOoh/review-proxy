namespace Repository.Interfaces;

public interface IAuditorQueryService
{
    Task<List<Guid>> GetAuditorsAsync(Guid repoId);
    Task<List<object>> GetAuditorsDetailsAsync(Guid repoId, string? authorizationHeader);
}
