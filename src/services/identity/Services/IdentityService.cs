using Identity.Data;
using Identity.Interfaces;
using Identity.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;

namespace Identity.Services;

public class IdentityService(
    IdentityDBContext context,
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<IdentityService> logger) : IIdentityService
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

    public async Task<List<UserEntry>> GetUsersAsync(string? query = null)
    {
        if (!string.IsNullOrWhiteSpace(query))
        {
            try
            {
                var client = httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");

                var response = await client.GetAsync($"https://api.github.com/search/users?q={query}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>();
                    if (content?.Items != null)
                    {
                        var tasks = content.Items.Select(async item =>
                        {
                            var dbUser = await GetUserByGitHubIdAsync(item.Id.ToString());
                            if (dbUser == null)
                            {
                                dbUser = await CreateUserAsync(item.Id.ToString(), item.Login, item.AvatarUrl);
                            }
                            return dbUser;
                        });

                        var ensuredUsers = await Task.WhenAll(tasks);
                        return ensuredUsers.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "GitHub user search failed for query '{Query}'. Falling back to local database.", query);
            }
        }

        var dbQuery = context.Set<UserEntry>().AsQueryable();
        if (!string.IsNullOrWhiteSpace(query))
        {
            dbQuery = dbQuery.Where(u => u.GitHubUsername.ToLower().Contains(query.ToLower()));
        }
        return await dbQuery.ToListAsync();
    }

    public async Task<(UserEntry User, string Token)> ExchangeAsync(string githubId, string username, string? avatarUrl)
    {
        var user = await GetUserByGitHubIdAsync(githubId);

        if (user == null)
        {
            user = await CreateUserAsync(githubId, username, avatarUrl);
        }
        else
        {
            user = await UpdateUserAsync(user.Id, username, avatarUrl);
        }

        var token = IssueJwt(user.Id, user.GitHubUsername);
        return (user, token);
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

    private string IssueJwt(Guid userId, string username)
    {
        var key = Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? "super_secret_key_that_is_long_enough_for_hmacsha256");

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            ]),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

internal class GitHubSearchResponse
{
    public List<GitHubUserItem> Items { get; set; } = [];
}

internal class GitHubUserItem
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}
