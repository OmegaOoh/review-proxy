using System.Text.Json;
using ReviewProxy.Contracts;
using Syncing.Interfaces.Services;

namespace Syncing.Services;

public class SyncingService(IHttpClientFactory httpClientFactory, IGitHubService githubService) : ISyncingService
{
    public async Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl, string? githubToken)
    {
        var client = httpClientFactory.CreateClient("identity");
        var response = await client.PostAsJsonAsync("/api/identities/exchange", new
        {
            GitHubId = githubId,
            Username = username,
            AvatarUrl = avatarUrl
        });

        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();

        return JsonSerializer.Serialize(new
        {
            user = result.GetProperty("user"),
            token = result.GetProperty("token").GetString(),
            github_token = githubToken
        });
    }

    public Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken) =>
        githubService.GetUserRepositoriesAsync(accessToken);

    public Task SyncIssueToGitHubAsync(IssueApprovalEvent approvalEvent) =>
        githubService.SyncIssueToGitHubAsync(approvalEvent);

    public Task<object> GetSyncContextAsync() =>
        githubService.GetSyncContextAsync();
}
