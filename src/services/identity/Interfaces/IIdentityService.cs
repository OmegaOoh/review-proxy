using Identity.Models;

namespace Identity.Interfaces;

public interface IIdentityService
{
    Task<UserEntry?> GetUserByIdAsync(Guid id);
    Task<UserEntry?> GetUserByGitHubIdAsync(string githubId);
    Task<UserEntry> CreateUserAsync(string githubId, string githubUsername, string? githubAvatarUrl);
    Task<UserEntry> UpdateUserAsync(Guid id, string githubUsername, string? githubAvatarUrl);
    Task<List<UserEntry>> GetUsersAsync(string? query = null);
    Task<(UserEntry User, string Token)> ExchangeAsync(string githubId, string username, string? avatarUrl);
    Task<UserEntry> EnsureUserAsync(string githubId, string username, string? avatarUrl);
}
