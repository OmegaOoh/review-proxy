using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Syncing.Interfaces;
using System.Security.Claims;

namespace Syncing.APIs;

public static class SyncingEndpoints
{
    public static void MapSyncingEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/sync");

        // Initiates the GitHub OAuth challenge.
        // After GitHub redirects back and the cookie is set, the browser lands on returnUrl.
        // The frontend is expected to call GET /api/sync/me (with the cookie) to complete the flow.
        group.MapGet("/signin", ([FromQuery] string? returnUrl = "/") =>
        {
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                new[] { "GitHub" });
        });

        group.MapGet("/signout", ([FromQuery] string? returnUrl = "/") =>
        {
            return Results.SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl },
                new[] { CookieAuthenticationDefaults.AuthenticationScheme });
        });

        // Called by the frontend after the OAuth cookie is set.
        // Reads the GitHub claims from the cookie session, forwards them to IdentityService
        // to upsert the user and issue a JWT, then returns { user, token } to the client.
        group.MapGet("/me", async (ClaimsPrincipal user, ISyncingService syncingService) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var githubId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
            var avatarUrl = user.FindFirst("urn:github:avatar")?.Value;

            if (githubId == null)
            {
                return Results.Unauthorized();
            }

            var json = await syncingService.ExchangeGitHubUserAsync(githubId, username, avatarUrl);
            return Results.Content(json, "application/json");

        }).RequireAuthorization();
    }
}
