using Identity.Models;

namespace Identity.Interfaces;

public interface IIdentityService
{
    Task<UserEntry?> GetUserByGitHubIdAsync(string githubId);
    Task<UserEntry> CreateUserAsync(string githubId, string githubUsername, string? githubAvatarUrl);
    Task<UserEntry> UpdateUserAsync(Guid id, string githubUsername, string? githubAvatarUrl);
}
