using System.Security.Claims;
using Identity.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Identity.APIs;

public static class UserEndpoints
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identities");

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

        group.MapGet("/{id:guid}", async (
            Guid id,
            IIdentityService identityService,
            HttpContext httpContext,
            IConfiguration configuration) =>
        {
            var configuredSecret = configuration["Services:InternalSecret"];
            var providedSecret = httpContext.Request.Headers["X-Internal-Secret"].FirstOrDefault();
            bool isInternal = !string.IsNullOrWhiteSpace(configuredSecret) && providedSecret == configuredSecret;

            if (!isInternal && httpContext.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var dbUser = await identityService.GetUserByIdAsync(id);
            return dbUser != null ? Results.Ok(dbUser) : Results.NotFound();
        });

        group.MapPost("/batch", async (
            [FromBody] List<Guid> ids,
            IIdentityService identityService,
            HttpContext httpContext,
            IConfiguration configuration) =>
        {
            var configuredSecret = configuration["Services:InternalSecret"];
            var providedSecret = httpContext.Request.Headers["X-Internal-Secret"].FirstOrDefault();
            bool isInternal = !string.IsNullOrWhiteSpace(configuredSecret) && providedSecret == configuredSecret;

            if (!isInternal && httpContext.User.Identity?.IsAuthenticated != true)
                return Results.Unauthorized();

            var users = await identityService.GetUsersByIdsAsync(ids);
            return Results.Ok(users);
        });

        group.MapGet("/", async ([FromQuery] string? query, IIdentityService identityService) =>
        {
            var users = await identityService.GetUsersAsync(query);
            return Results.Ok(users);
        }).RequireAuthorization();
    }
}
