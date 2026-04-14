using GitHub;
using GitHub.Models;
using Syncing.Interfaces.Clients;
using Syncing.Interfaces.Services;

namespace Syncing.Services;

public class GitHubInstallationService(
    IConfiguration configuration,
    IGitHubClientFactory clientFactory) : IGitHubInstallationService
{
    private const int DefaultPageSize = 100;
    private const int StartPage = 1;

    public async Task<string> GetInstallationTokenAsync(string owner)
    {
        var appId = configuration["GitHub:AppId"] ?? throw new InvalidOperationException("AppId not set");
        var keyPath = configuration["GitHub:PrivateKeyPath"] ?? throw new InvalidOperationException("KeyPath not set");

        var jwt = clientFactory.GenerateGitHubJwt(appId, await File.ReadAllTextAsync(keyPath));
        var github = clientFactory.CreateGitHubClient(jwt);

        var installations = await github.App.Installations.GetAsync();

        var inst = installations?.FirstOrDefault(i =>
            i.Account?.SimpleUser?.Login?.Equals(owner, StringComparison.OrdinalIgnoreCase) == true);

        if (inst?.Id == null) throw new InvalidOperationException($"No installation found for owner {owner}");

        var requestBody = new GitHub.App.Installations.Item.Access_tokens.Access_tokensPostRequestBody();
        var tokenResponse = await github.App.Installations[(int)inst.Id.Value]
            .Access_tokens
            .PostAsync(requestBody);

        if (string.IsNullOrEmpty(tokenResponse?.Token)) throw new InvalidOperationException("Token creation failed");

        return tokenResponse.Token;
    }

    public async Task<(List<Repository> Repos, HashSet<long> InstalledRepoIds)> GetUserInstallationReposAsync(string accessToken)
    {
        var github = clientFactory.CreateGitHubClient(accessToken);
        var allRepos = new List<Repository>();
        var installedRepoIds = new HashSet<long>();

        int page = StartPage;
        while (true)
        {
            var response = await github.User.Installations.GetAsync(p =>
            {
                p.QueryParameters.Page = page;
                p.QueryParameters.PerPage = DefaultPageSize;
            });

            if (response?.Installations == null || !response.Installations.Any()) break;

            foreach (var inst in response.Installations)
                await FetchInstallationReposAsync(github, inst.Id ?? 0, allRepos, installedRepoIds);

            if (response.Installations.Count < DefaultPageSize) break;
            page++;
        }

        return (allRepos, installedRepoIds);
    }

    private async Task FetchInstallationReposAsync(GitHubClient github, long installationId, List<Repository> allRepos, HashSet<long> installedRepoIds)
    {
        int page = StartPage;
        while (true)
        {
            var response = await github.User.Installations[(int)installationId].Repositories.GetAsync(p =>
            {
                p.QueryParameters.Page = page;
                p.QueryParameters.PerPage = DefaultPageSize;
            });

            if (response?.Repositories == null || !response.Repositories.Any()) break;

            foreach (var repo in response.Repositories)
            {
                if (repo.Id.HasValue) installedRepoIds.Add(repo.Id.Value);
            }
            allRepos.AddRange(response.Repositories);

            if (response.Repositories.Count < DefaultPageSize) break;
            page++;
        }
    }

    public async Task<object> GetSyncContextAsync()
    {
        var appSlug = configuration["GitHub:AppSlug"] ?? "review-proxy";
        return new
        {
            app_slug = appSlug,
            installation_url = $"https://github.com/apps/{appSlug}/installations/new"
        };
    }
}
