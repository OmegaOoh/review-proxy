using GitHub.Models;

namespace Syncing.Interfaces.Services;

public interface IGitHubInstallationService
{
    Task<string> GetInstallationTokenAsync(string owner);
    Task<(List<Repository> Repos, HashSet<long> InstalledRepoIds)> GetUserInstallationReposAsync(string accessToken);
    Task<object> GetSyncContextAsync();
}
