using Repository.Interfaces.Services;

namespace Repository.Services;

public class GitHubRepositoryService(IHttpClientFactory httpClientFactory) : IGitHubRepositoryService
{
    private const string GitHubApiUrl = "https://api.github.com/repos/";
    private const string UserAgent = "ReviewProxy";
    private const string GitHubApiVersion = "2026-03-10";

    public async Task<(string FullName, bool HasAccess)> ValidateRepositoryAsync(string githubRepoId, string? githubToken)
    {
        if (string.IsNullOrWhiteSpace(githubRepoId))
        {
            throw new ArgumentException("GitHub Repository ID is required.");
        }

        var client = httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", GitHubApiVersion);

        if (!string.IsNullOrWhiteSpace(githubToken))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {githubToken}");
        }

        var response = await client.GetAsync($"{GitHubApiUrl}{githubRepoId}");
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException("Invalid or inaccessible GitHub repository.");
        }

        var json = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();
        var fullName = json.GetProperty("full_name").GetString() ?? githubRepoId;

        if (json.TryGetProperty("permissions", out var permissions))
        {
            var admin = permissions.TryGetProperty("admin", out var a) && a.GetBoolean();
            var push = permissions.TryGetProperty("push", out var p) && p.GetBoolean();

            if (!admin && !push)
            {
                throw new ArgumentException("You must have admin or push access to the repository on GitHub.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(githubToken))
        {
            // If authenticated but permissions not returned, GitHub likely doesn't see us as having access
            throw new ArgumentException("Unable to verify repository permissions with GitHub.");
        }

        return (fullName, true);
    }
}
