using Identity.Data;
using Identity.Interfaces;
using Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Services;

public class IdentityService(IdentityDBContext context) : IIdentityService
{
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
        var user = await context.Set<UserEntry>().FindAsync(id);

        if (user == null)
        {
            throw new Exception("User not found");
        }

        user.GitHubUsername = githubUsername;
        user.GitHubAvatarUrl = githubAvatarUrl;

        context.Set<UserEntry>().Update(user);
        await context.SaveChangesAsync();

        return user;
    }
}
