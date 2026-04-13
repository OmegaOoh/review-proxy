using Syncing.Models;

namespace Syncing.Interfaces;

public interface IIdentityClient
{
    Task<GitHubUser?> GetGitHubUserByIdAsync(string userId);
    Task<GitHubUser?> GetGitHubUserByGitHubIdAsync(string githubId);
}
