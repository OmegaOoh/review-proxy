using Syncing.Interfaces;
using System.Net.Http.Json;

namespace Syncing.Services;

public class SyncingService(IHttpClientFactory httpClientFactory) : ISyncingService
{
    public async Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl)
    {
        var client = httpClientFactory.CreateClient("identity");

        var response = await client.PostAsJsonAsync("/api/identities/exchange", new
        {
            GitHubId = githubId,
            Username = username,
            AvatarUrl = avatarUrl
        });

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
