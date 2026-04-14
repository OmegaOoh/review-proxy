using Identity.Data;
using Identity.Interfaces.Services;
using Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class UserService(IdentityDBContext context) : IUserService
{
    public async Task<UserEntry?> GetUserByIdAsync(Guid id)
    {
        return await context.Set<UserEntry>().FindAsync(id);
    }

    public async Task<List<UserEntry>> GetUsersByIdsAsync(List<Guid> ids)
    {
        return await context.Set<UserEntry>()
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
    }

    public async Task<UserEntry?> GetUserByGitHubIdAsync(string githubId)
    {
        return await context.Set<UserEntry>().FirstOrDefaultAsync(u => u.GitHubID == githubId);
    }

    public async Task<UserEntry> CreateUserAsync(string githubId, string githubUsername, string? githubAvatarUrl)
    {
        var user = new UserEntry
        {
            Id = Guid.NewGuid(),
            GitHubID = githubId,
            GitHubUsername = githubUsername,
            GitHubAvatarUrl = githubAvatarUrl
        };

        context.Set<UserEntry>().Add(user);
        await context.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntry> UpdateUserAsync(Guid id, string githubUsername, string? githubAvatarUrl)
    {
        var user = await context.Set<UserEntry>().FindAsync(id)
            ?? throw new Exception("User not found");

        user.GitHubUsername = githubUsername;
        user.GitHubAvatarUrl = githubAvatarUrl;

        context.Set<UserEntry>().Update(user);
        await context.SaveChangesAsync();

        return user;
    }

    public async Task<UserEntry> EnsureUserAsync(string githubId, string username, string? avatarUrl)
    {
        var user = await GetUserByGitHubIdAsync(githubId);
        if (user == null)
        {
            user = await CreateUserAsync(githubId, username, avatarUrl);
        }
        return user;
    }

    public async Task<List<UserEntry>> GetUsersByQueryAsync(string? query)
    {
        var dbQuery = context.Set<UserEntry>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(u => u.GitHubUsername.ToLower().Contains(query.ToLower()));
        }
        return await dbQuery.ToListAsync();
    }
}
