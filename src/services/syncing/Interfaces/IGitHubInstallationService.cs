using GitHub;
using GitHub.Models;

namespace Syncing.Interfaces;

public interface IGitHubInstallationService
{
    Task<string> GetInstallationTokenAsync(string owner);
    Task<(List<Repository> Repos, HashSet<long> InstalledRepoIds)> GetUserInstallationReposAsync(string accessToken);
    Task<object> GetSyncContextAsync();
}
