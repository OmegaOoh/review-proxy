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
            try
            {
                var updatedIssue = await issueService.EditIssueAsync(id, userId, issuePatch);
                return Results.Ok(updatedIssue);
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (Exception)
            {
                // Fallback for unexpected server errors
                return Results.StatusCode(500);
            }


        })
        .WithName("EditIssue")
        ;

        group.MapPost("/{id:guid}/approve", async (Guid id, IIssueService issueService, HttpContext context) =>
            {
                var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? context.User.FindFirst("sub")?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return Results.Unauthorized();
                }

                try
                {
                    await issueService.ApproveIssueAsync(id, userId);
                    return Results.Ok();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(new { message = ex.Message });
                }
                catch (InvalidOperationException ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
                catch (UnauthorizedAccessException)
                {
                    return Results.Forbid();
                }
                catch (Exception)
                {
                    return Results.StatusCode(500);
                }
            })
            .WithName("ApproveIssue")
            ;

        group.MapPost("/{id:guid}/reject", async (Guid id, IIssueService issueService, HttpContext context) =>
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                         ?? context.User.FindFirst("sub")?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }
            try
            {
                await issueService.RejectIssueAsync(id, userId);
                return Results.Ok();
            }
            catch (KeyNotFoundException ex)
            {
                return Results.NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Results.Forbid();
            }
            catch (Exception)
            {
                return Results.StatusCode(500);
            }
        })
        .WithName("RejectIssue")
        ;

        return app;
    }
}
