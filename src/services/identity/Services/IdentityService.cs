using Identity.Interfaces;
using Identity.Models;

namespace Identity.Services;

public class IdentityService(
    IUserService userService,
    IGitHubIdentityClient githubClient,
    ITokenService tokenService,
    ILogger<IdentityService> logger) : IIdentityService
{
    public async Task<UserEntry?> GetUserByIdAsync(Guid id) => await userService.GetUserByIdAsync(id);

    public async Task<List<UserEntry>> GetUsersByIdsAsync(List<Guid> ids) => await userService.GetUsersByIdsAsync(ids);

    public async Task<UserEntry?> GetUserByGitHubIdAsync(string githubId) => await userService.GetUserByGitHubIdAsync(githubId);

    public async Task<UserEntry> CreateUserAsync(string githubId, string githubUsername, string? githubAvatarUrl)
        => await userService.CreateUserAsync(githubId, githubUsername, githubAvatarUrl);

    public async Task<UserEntry> UpdateUserAsync(Guid id, string githubUsername, string? githubAvatarUrl)
        => await userService.UpdateUserAsync(id, githubUsername, githubAvatarUrl);

    public async Task<List<UserEntry>> GetUsersAsync(string? query = null)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                var githubUsers = await githubClient.SearchUsersAsync(query);
                if (githubUsers.Count > 0)
                {
                    var tasks = githubUsers.Select(async item =>
                    {
                        return await userService.EnsureUserAsync(item.Id.ToString(), item.Login, item.AvatarUrl);
                    });

                    var ensuredUsers = await Task.WhenAll(tasks);
                    return ensuredUsers.ToList();
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GitHub user search failed for query '{Query}'. Falling back to local database.", query);
            }
        }

        // Fallback or just query local DB if no query or if it failed.
        // Re-using the logic from original:
        // var dbQuery = context.Set<UserEntry>().AsQueryable();
        // if (!string.IsNullOrWhiteSpace(query)) { ... }
        // return await dbQuery.ToListAsync();

        // But we need a way to do this in UserService or directly.
        // Let's add a SearchLocalUsersAsync to IUserService.
        return await SearchLocalUsersAsync(query);
    }

    private async Task<List<UserEntry>> SearchLocalUsersAsync(string? query)
    {
        // I'll add this to UserService instead for better separation.
        // For now, I'll just call it on userService if I add it there.
        return await userService.GetUsersByQueryAsync(query);
    }

    public async Task<(UserEntry User, string Token)> ExchangeAsync(string githubId, string username, string? avatarUrl)
    {
        var user = await userService.GetUserByGitHubIdAsync(githubId);

        if (user == null)
        {
            user = await userService.CreateUserAsync(githubId, username, avatarUrl);
        }
        else
        {
            user = await userService.UpdateUserAsync(user.Id, username, avatarUrl);
        }

        var token = tokenService.IssueJwt(user.Id, user.GitHubUsername);
        return (user, token);
    }

    public async Task<UserEntry> EnsureUserAsync(string githubId, string username, string? avatarUrl)
    {
        return await userService.EnsureUserAsync(githubId, username, avatarUrl);
    }
}
