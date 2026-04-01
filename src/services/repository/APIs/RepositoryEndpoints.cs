using Repository.Interfaces;
using Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Repository.Models.Dtos;
using System.Security.Claims;
using System.Net.Http;

namespace Repository.APIs;

public static class RepositoryEndpoints
{
    public static void MapRepositoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/repositories");

        group.MapGet("/", async (IRepositoryService service,
            [FromQuery] Guid? ownerId,
            [FromQuery] RepositoryRole? role) =>
        {
            var results = await service.GetRepositoriesAsync(ownerId, role);

            if (ownerId.HasValue && results.Count == 0) return Results.NotFound();

            return Results.Ok(results);
        });

        group.MapPost("/", async (
            IRepositoryService service,
            [FromBody] DepositRequest depot,
            ClaimsPrincipal user,
            IHttpClientFactory httpClientFactory) =>
        {
            var ownerIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim)) return Results.Unauthorized();

            var usernameClaim = user.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(usernameClaim)) return Results.Unauthorized();

            var repoId = depot.GithubRepoId;
            if (string.IsNullOrWhiteSpace(repoId) || !repoId.StartsWith(usernameClaim + "/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Repository must be owned by the authenticated user.");
            }

            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");
            var response = await client.GetAsync($"https://api.github.com/repos/{repoId}");
            if (!response.IsSuccessStatusCode)
            {
                return Results.BadRequest("Invalid or inaccessible GitHub repository.");
            }

            var id = await service.DepositAsync(depot.GithubRepoId, ownerIdClaim, depot.Description, depot.Auditors);
            return Results.Created($"/api/repositories/{id}", id);
        }).RequireAuthorization();

        group.MapPatch("/{id}", async (IRepositoryService service, Guid id, [FromBody] RepositoryPatchRequest patch) =>
        {
            await service.UpdateRepositoryAsync(id, patch.Description);
            return Results.NoContent();
        });

        group.MapPost("/{id}/auditors", async (IAuditorService service, Guid id, [FromBody] List<Guid> auditors) =>
        {
            await service.AddAuditorsAsync(id, auditors);
            return Results.NoContent();
        });

        group.MapDelete("/{id}/auditors", async (IAuditorService service, Guid id, [FromBody] List<Guid> auditors) =>
        {
            await service.RemoveAuditorsAsync(id, auditors);
            return Results.NoContent();
        });

        group.MapGet("/{id}/auditors", async (IAuditorService service, Guid id) =>
        {
            var auditors = await service.GetAuditorsAsync(id);
            return Results.Ok(auditors);
        });
    }
}
