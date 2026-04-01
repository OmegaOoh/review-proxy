namespace Syncing.Interfaces;

public interface ISyncingService
{
    Task<string> ExchangeGitHubUserAsync(string githubId, string username, string? avatarUrl);
}
