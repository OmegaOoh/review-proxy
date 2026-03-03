using Repository.Interfaces;
using Repository.Models;
using Microsoft.AspNetCore.Mvc;
using Repository.Models.Dtos;

namespace Repository.Services;

public static class RepositoryServices
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

        group.MapPost("/", async (IRepositoryService service, [FromBody] DepositRequest depot) =>
        {
            var id = await service.DepositAsync(depot.GithubRepoId, depot.OwnerId, depot.Description);
            return Results.Created($"/api/repositories/{id}", id);
        });

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
