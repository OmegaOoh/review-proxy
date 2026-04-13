using Identity.Interfaces;
using Identity.Models.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Identity.APIs;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identities");

        // Internal service-to-service endpoint called by SyncingService after GitHub OAuth completes.
        // Upserts the user from GitHub claims and returns the user record alongside a signed JWT.
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
    }
}
