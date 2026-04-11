using Syncing.Interfaces;
using System.Net.Http.Json;
using Octokit;
using GitHubJwt;
using Microsoft.Extensions.Configuration;

namespace Syncing.Services;

public class SyncingService(IHttpClientFactory httpClientFactory, IConfiguration configuration) : ISyncingService
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

        // The identity service returns { User: ..., Token: ... }
        // We parse and re-emit as camelCase { user: ..., token: ... } for the frontend.
        var result = await response.Content.ReadFromJsonAsync<System.Text.Json.JsonElement>();

        return System.Text.Json.JsonSerializer.Serialize(new
        {
            user = result.GetProperty("user"),
            token = result.GetProperty("token").GetString()
        });
    }

    public async Task<IEnumerable<object>> GetUserRepositoriesAsync(string accessToken)
    {
        var github = new GitHubClient(new ProductHeaderValue("ReviewProxy"))
        {
            Credentials = new Credentials(accessToken)
        };

        try
        {
            // 1. Get user installations for this GitHub App
            // API: GET /user/installations
            var installationsResponse = await github.GitHubApps.GetAllInstallationsForCurrentUser();

            var allRepos = new List<Repository>();

            foreach (var installation in installationsResponse.Installations)
            {
                // 2. For each installation, get repositories accessible to this user
                // API: GET /user/installations/{installation_id}/repositories
                var reposResponse = await github.GitHubApps.Installation.GetAllRepositoriesForCurrentUser(installation.Id);
                allRepos.AddRange(reposResponse.Repositories);
            }

            // If we found repos via installations, return them.
            // This is more efficient because it's scoped to what the App can see.
            if (allRepos.Any())
            {
                return allRepos.DistinctBy(r => r.Id).Select(r => new
                {
                    id = r.Id,
                    name = r.Name,
                    full_name = r.FullName,
                    description = r.Description,
                    html_url = r.HtmlUrl,
                    @private = r.Private
                });
            }
        }
        catch (Exception ex)
        {
            // Log error or fall back
            Console.WriteLine($"Error fetching via installations: {ex.Message}");
        }
        // Fallback: List all repositories the user has access to directly
        // This might be less efficient if the user has thousands of repos,
        // but it works even if they haven't installed the app yet.
        var userRepos = await github.Repository.GetAllForCurrent();
        return userRepos.Select(r => new
        {
            id = r.Id,
            name = r.Name,
            full_name = r.FullName,
            description = r.Description,
            html_url = r.HtmlUrl,
            @private = r.Private
        });
    }
    private async Task<string> GetInstallationTokenAsync(long installationId)
    {
        var appId = configuration["GitHub:AppId"];
        var privateKey = configuration["GitHub:PrivateKey"];

        if (string.IsNullOrEmpty(appId) || string.IsNullOrEmpty(privateKey))
        {
            throw new InvalidOperationException("GitHub AppId or PrivateKey is not configured.");
        }

        // The PrivateKey might contain literal \n if it was loaded from env vars or some configs.
        // We ensure it has actual newlines.
        var formattedPrivateKey = privateKey.Replace("\\n", "\n");

        var generator = new GitHubJwtFactory(
            new StringPrivateKeySource(formattedPrivateKey),
            new GitHubJwtFactoryOptions
            {
                AppIntegrationId = int.Parse(appId),
                ExpirationSeconds = 600 // 10 minutes
            }
        );

        var jwt = generator.CreateEncodedJwtToken();

        var appClient = new GitHubClient(new ProductHeaderValue("ReviewProxy-App"))
        {
            Credentials = new Credentials(jwt, AuthenticationType.Bearer)
        };

        var response = await appClient.GitHubApps.CreateInstallationToken(installationId);
        return response.Token;
    }
}
