using System.Net.Http.Json;
using Repository.Interfaces;

namespace Repository.Services;

public class IdentityClient(IHttpClientFactory httpClientFactory) : IIdentityClient
{
    private const string IdentityClientName = "identity";
    private const string BatchEndpoint = "/api/identities/batch";

    public async Task<List<object>> GetUsersBatchAsync(List<Guid> userIds, string? authorizationHeader)
    {
        if (userIds.Count == 0) return [];

        var client = httpClientFactory.CreateClient(IdentityClientName);
        if (!string.IsNullOrEmpty(authorizationHeader))
        {
            client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", authorizationHeader);
        }

        try
        {
            var response = await client.PostAsJsonAsync(BatchEndpoint, userIds);
            if (response.IsSuccessStatusCode)
            {
                var details = await response.Content.ReadFromJsonAsync<List<object>>();
                return details ?? [];
            }
        }
        catch
        {
            // Fallback to empty if batch fails
            return [];
        }

        return [];
    }
}
