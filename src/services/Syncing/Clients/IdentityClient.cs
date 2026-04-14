using Syncing.Interfaces.Services;
using Syncing.Models;

namespace Syncing.Clients;

public class IdentityClient(IHttpClientFactory httpClientFactory, ILogger<IdentityClient> logger) : IIdentityClient
{
    public async Task<GitHubUser?> GetGitHubUserByIdAsync(string userId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("identity");
            return await client.GetFromJsonAsync<GitHubUser>($"/api/identities/{userId}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch user {UserId} by internal ID from identity service", userId);
            return null;
        }
    }

    public async Task<GitHubUser?> GetGitHubUserByGitHubIdAsync(string githubId)
    {
        try
        {
            var client = httpClientFactory.CreateClient("identity");
            return await client.GetFromJsonAsync<GitHubUser>($"/api/identities/github/{githubId}");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to fetch user {GitHubId} by GitHub ID from identity service", githubId);
            return null;
        }
    }
}
