namespace Repository.Interfaces.Clients;

public interface IIdentityClient
{
    Task<List<object>> GetUsersBatchAsync(List<Guid> userIds, string? authorizationHeader);
}
