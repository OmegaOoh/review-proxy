using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Issue.Interfaces;
using Issue.Models;
using Issue.Models.Dtos;

namespace Issue.APIs;

public static class IssueEndpoints
{
    public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/issues").RequireAuthorization();

        group.MapGet("/", async (IIssueService issueService) =>
        {
            var issues = await issueService.GetAllIssuesAsync();
            return Results.Ok(issues);
        })
        .WithName("GetIssues")
        ;

        group.MapPost("/", async (IssueEntry issue, IIssueService issueService, HttpContext context) =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            // Enforce the owner ID to be the currently authenticated user
            issue.OwnerId = userId;

            var createdIssue = await issueService.CreateIssueAsync(issue);
            return Results.Created($"/api/issues/{createdIssue.Id}", createdIssue);
        })
        .WithName("CreateIssue")
        ;

        group.MapDelete("/{id:guid}", async (Guid id, IIssueService issueService, HttpContext context) =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var deleted = await issueService.DeleteIssueAsync(id, userId);

            if (!deleted)
            {
                return Results.NotFound(new { Message = "Issue not found or you do not have permission to delete it." });
            }

            return Results.NoContent();
        })
        .WithName("DeleteIssue")
        ;

        group.MapPut("/{id:guid}", async (Guid id, IssuePatchRequest issuePatch, IIssueService issueService, HttpContext context) =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var updatedIssue = await issueService.EditIssueAsync(id, userId, issuePatch);

            if (updatedIssue == null)
            {
                return Results.NotFound(new { Message = "Issue not found or you do not have permission to edit it." });
            }

            return Results.Ok(updatedIssue);
        })
        .WithName("EditIssue")
        ;
        

        return app;
    }
}
