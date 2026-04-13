namespace Repository.Interfaces;

public interface IIdentityClient
{
    Task<List<object>> GetUsersBatchAsync(List<Guid> userIds, string? authorizationHeader);
}
