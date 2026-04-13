using Identity.Models;

namespace Identity.Interfaces;

public interface IUserService
{
    Task<UserEntry?> GetUserByIdAsync(Guid id);
    Task<List<UserEntry>> GetUsersByIdsAsync(List<Guid> ids);
    Task<UserEntry?> GetUserByGitHubIdAsync(string githubId);
    Task<UserEntry> CreateUserAsync(string githubId, string githubUsername, string? githubAvatarUrl);
    Task<UserEntry> UpdateUserAsync(Guid id, string githubUsername, string? githubAvatarUrl);
    Task<UserEntry> EnsureUserAsync(string githubId, string username, string? avatarUrl);
    Task<List<UserEntry>> GetUsersByQueryAsync(string? query);
}
