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

        group.MapGet("/", GetIssues).WithName("GetIssues");
        group.MapPost("/", CreateIssue).WithName("CreateIssue");
        group.MapDelete("/{id:guid}", DeleteIssue).WithName("DeleteIssue");
        group.MapPut("/{id:guid}", EditIssue).WithName("EditIssue");
        group.MapPost("/{id:guid}/approve", ApproveIssue).WithName("ApproveIssue");
        group.MapPost("/{id:guid}/reject", RejectIssue).WithName("RejectIssue");

        return app;
    }

    private static async Task<IResult> GetIssues(IIssueService issueService)
    {
        var issues = await issueService.GetAllIssuesAsync();
        return Results.Ok(issues);
    }

    private static async Task<IResult> CreateIssue(IssueEntry issue, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        issue.OwnerId = userId;
        var createdIssue = await issueService.CreateIssueAsync(issue);
        return Results.Created($"/api/issues/{createdIssue.Id}", createdIssue);
    }

    private static async Task<IResult> DeleteIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var deleted = await issueService.DeleteIssueAsync(id, userId);
        return deleted ? Results.NoContent() : Results.NotFound(new { Message = "Issue not found or you do not have permission to delete it." });
    }

    private static async Task<IResult> EditIssue(Guid id, IssuePatchRequest issuePatch, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            var updatedIssue = await issueService.EditIssueAsync(id, userId, issuePatch);
            return Results.Ok(updatedIssue);
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static async Task<IResult> ApproveIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            await issueService.ApproveIssueAsync(id, userId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static async Task<IResult> RejectIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            await issueService.RejectIssueAsync(id, userId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return HandleException(ex);
        }
    }

    private static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

    private static IResult HandleException(Exception ex) => ex switch
    {
        KeyNotFoundException => Results.NotFound(new { message = ex.Message }),
        InvalidOperationException => Results.BadRequest(new { message = ex.Message }),
        UnauthorizedAccessException => Results.Forbid(),
        _ => Results.StatusCode(500)
    };
}
