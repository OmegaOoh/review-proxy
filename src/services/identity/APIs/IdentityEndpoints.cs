using Identity.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Identity.APIs;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identities");

        // Internal service-to-service endpoint called by SyncingService after GitHub OAuth completes.
        // Upserts the user from GitHub claims and returns the user record alongside a signed JWT.
        // Protected by a shared-secret header (X-Internal-Secret). Configure Services:InternalSecret
        // in appsettings. When left empty the check is skipped (development convenience).
        group.MapPost("/exchange", async (
            ExchangeRequest request,
            IIdentityService identityService,
            HttpContext httpContext,
            IConfiguration configuration) =>
        {
            var configuredSecret = configuration["Services:InternalSecret"];
            if (!string.IsNullOrWhiteSpace(configuredSecret))
            {
                var providedSecret = httpContext.Request.Headers["X-Internal-Secret"].FirstOrDefault();
                if (string.IsNullOrWhiteSpace(providedSecret) || providedSecret != configuredSecret)
                    return Results.Unauthorized();
            }

            if (string.IsNullOrWhiteSpace(request.GitHubId))
                return Results.BadRequest("GitHubId is required.");

            var (user, token) = await identityService.ExchangeAsync(request.GitHubId, request.Username, request.AvatarUrl);
            return Results.Ok(new { User = user, Token = token });
        });

        group.MapPost("/ensure", async (EnsureRequest request, IIdentityService identityService) =>
        {
            if (string.IsNullOrWhiteSpace(request.GitHubId))
                return Results.BadRequest("GitHubId is required.");

            var user = await identityService.EnsureUserAsync(request.GitHubId, request.Username, request.AvatarUrl);
            return Results.Ok(user);
        }).RequireAuthorization();

        // JWT-protected endpoint for already-authenticated clients to retrieve their own profile.
        group.MapGet("/me", async (ClaimsPrincipal user, IIdentityService identityService) =>
        {
            if (user.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userIdClaim == null || !Guid.TryParse(userIdClaim, out var userId))
                return Results.Unauthorized();

            var dbUser = await identityService.GetUserByIdAsync(userId);

            if (dbUser == null)
                return Results.NotFound();

            return Results.Ok(dbUser);
        }).RequireAuthorization();

        group.MapGet("/{id:guid}", async (Guid id, IIdentityService identityService) =>
        {
            var dbUser = await identityService.GetUserByIdAsync(id);

            if (dbUser == null)
                return Results.NotFound();

            return Results.Ok(dbUser);
        }).RequireAuthorization();

        group.MapPost("/batch", async ([FromBody] List<Guid> ids, IIdentityService identityService) =>
        {
            var users = await identityService.GetUsersByIdsAsync(ids);
            return Results.Ok(users);
        }).RequireAuthorization();

        group.MapGet("/", async ([FromQuery] string? query, IIdentityService identityService) =>
        {
            var users = await identityService.GetUsersAsync(query);
            return Results.Ok(users);
        }).RequireAuthorization();
    }
}

public record ExchangeRequest(string GitHubId, string Username, string? AvatarUrl);
public record EnsureRequest(string GitHubId, string Username, string? AvatarUrl);
