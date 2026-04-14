using Syncing.Models;

namespace Syncing.Interfaces.Services;

public interface IIdentityClient
{
    Task<GitHubUser?> GetGitHubUserByIdAsync(string userId);
    Task<GitHubUser?> GetGitHubUserByGitHubIdAsync(string githubId);
}
