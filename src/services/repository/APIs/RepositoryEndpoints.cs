using Repository.Interfaces;
using Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Repository.Models.Dtos;
using System.Security.Claims;

namespace Repository.APIs;

public static class RepositoryEndpoints
{
    public static void MapRepositoryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/repositories").RequireAuthorization();

        group.MapGet("/", async (IRepositoryService service,
            [FromQuery] Guid? ownerId,
            [FromQuery] RepositoryRole? role) =>
        {
            var results = await service.GetRepositoriesAsync(ownerId, role);

            return Results.Ok(results);
        });

        group.MapPost("/", async (
            IRepositoryService service,
            [FromBody] DepositRequest depot,
            ClaimsPrincipal user) =>
        {
            var ownerIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim)) return Results.Unauthorized();

            try
            {
                var id = await service.DepositAsync(depot.GithubRepoId, ownerIdClaim, depot.Description, depot.Auditors);
                return Results.Created($"/api/repositories/{id}", id);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(ex.Message);
            }
            catch (HttpRequestException)
            {
                return Results.BadRequest("Invalid or inaccessible GitHub repository.");
            }
        });

        group.MapPatch("/{id}", async (IRepositoryService service, Guid id, [FromBody] RepositoryPatchRequest patch) =>
        {
            await service.UpdateRepositoryAsync(id, patch.Description);
            return Results.NoContent();
        });

        group.MapDelete("/{id:guid}", async (IRepositoryService service, Guid id, ClaimsPrincipal user) =>
        {
            var ownerIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(ownerIdClaim)) return Results.Unauthorized();

            var deleted = await service.DeleteRepositoryAsync(id, ownerIdClaim);
            return deleted ? Results.NoContent() : Results.NotFound();
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

        group.MapGet("/{id}/auditors/details", async (IAuditorService service, Guid id, HttpContext httpContext) =>
        {
            var authorizationHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
            var details = await service.GetAuditorsDetailsAsync(id, authorizationHeader);
            return Results.Ok(details);
        });
    }
}
