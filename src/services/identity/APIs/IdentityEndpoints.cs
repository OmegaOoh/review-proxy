using Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Identity.APIs;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app, IConfiguration configuration)
    {
        var group = app.MapGroup("/api/identities");

        // Internal service-to-service endpoint called by SyncingService after GitHub OAuth completes.
        // Upserts the user from GitHub claims and returns the user record alongside a signed JWT.
        // NOTE: this endpoint is intentionally not Bearer-protected because the caller (SyncingService)
        // is a trusted internal peer. In production, add a shared-secret header or mTLS.
        group.MapPost("/exchange", async (ExchangeRequest request, IIdentityService identityService) =>
        {
            if (string.IsNullOrWhiteSpace(request.GitHubId))
            {
                return Results.BadRequest("GitHubId is required.");
            }

            var dbUser = await identityService.GetUserByGitHubIdAsync(request.GitHubId);

            if (dbUser == null)
            {
                dbUser = await identityService.CreateUserAsync(
                    request.GitHubId,
                    request.Username,
                    request.AvatarUrl);
            }
            else
            {
                dbUser = await identityService.UpdateUserAsync(
                    dbUser.Id,
                    request.Username,
                    request.AvatarUrl);
            }

            var token = IssueJwt(dbUser.Id, dbUser.GitHubUsername, configuration);

            return Results.Ok(new { User = dbUser, Token = token });
        });

        group.MapPost("/ensure", async (EnsureRequest request, IIdentityService identityService) =>
        {
            if (string.IsNullOrWhiteSpace(request.GitHubId))
            {
                return Results.BadRequest("GitHubId is required.");
            }

            var dbUser = await identityService.GetUserByGitHubIdAsync(request.GitHubId);

            if (dbUser == null)
            {
                dbUser = await identityService.CreateUserAsync(
                    request.GitHubId,
                    request.Username,
                    request.AvatarUrl);
            }

            return Results.Ok(dbUser);
        }).RequireAuthorization();

        // JWT-protected endpoint for already-authenticated clients to retrieve their own profile.
        group.MapGet("/me", async (ClaimsPrincipal user, IIdentityService identityService) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Results.Unauthorized();
            }

            var dbUser = await identityService.GetUserByIdAsync(userId);

            if (dbUser == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(dbUser);

        }).RequireAuthorization();

        // Endpoint to get all users
        group.MapGet("/", async ([FromQuery] string? query, IIdentityService identityService) =>
        {
            if (!string.IsNullOrWhiteSpace(query))
            {
                // Proxy to github users
                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");

                try
                {
                    var response = await client.GetAsync($"https://api.github.com/search/users?q={query}");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadFromJsonAsync<GitHubSearchResponse>();
                        if (content?.Items != null)
                        {
                            var tasks = content.Items.Select(async item =>
                            {
                                var dbUser = await identityService.GetUserByGitHubIdAsync(item.Id.ToString());
                                if (dbUser == null)
                                {
                                    dbUser = await identityService.CreateUserAsync(item.Id.ToString(), item.Login, item.AvatarUrl);
                                }
                                return dbUser;
                            });

                            var ensuredUsers = await Task.WhenAll(tasks);
                            return Results.Ok(ensuredUsers);
                        }
                    }
                }
                catch
                {
                    // Fallback to local DB if github fails
                }
            }

            var users = await identityService.GetUsersAsync(query);
            return Results.Ok(users);
        }).RequireAuthorization();
    }

    private static string IssueJwt(Guid userId, string username, IConfiguration configuration)
    {
        var key = Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? "super_secret_key_that_is_long_enough_for_hmacsha256");

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            }),
            Expires = DateTime.UtcNow.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

// Request body for the /exchange endpoint
public record ExchangeRequest(string GitHubId, string Username, string? AvatarUrl);

public record EnsureRequest(string GitHubId, string Username, string? AvatarUrl);

public class GitHubSearchResponse
{
    public List<GitHubUserItem> Items { get; set; } = new();
}

public class GitHubUserItem
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}
