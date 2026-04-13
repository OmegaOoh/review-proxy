using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Issue.Interfaces;
using Issue.Models;
using Issue.Models.Dtos;

namespace Issue.APIs;

internal static class IssueManagementEndpoints
{
    public static void MapManagementEndpoints(this IEndpointRouteBuilder group)
    {
        group.MapGet("/", GetIssues).WithName("GetIssues");
        group.MapPost("/", CreateIssue).WithName("CreateIssue");
        group.MapDelete("/{id:guid}", DeleteIssue).WithName("DeleteIssue");
        group.MapPut("/{id:guid}", EditIssue).WithName("EditIssue");
    }

    private static async Task<IResult> GetIssues(IIssueService issueService)
    {
        var issues = await issueService.GetAllIssuesAsync();
        return Results.Ok(issues);
    }

    private static async Task<IResult> CreateIssue(IssueEntry issue, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = IssueEndpoints.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        issue.OwnerId = userId;
        var createdIssue = await issueService.CreateIssueAsync(issue);
        return Results.Created($"/api/issues/{createdIssue.Id}", createdIssue);
    }

    private static async Task<IResult> DeleteIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = IssueEndpoints.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        var deleted = await issueService.DeleteIssueAsync(id, userId);
        return deleted ? Results.NoContent() : Results.NotFound(new { Message = "Issue not found or you do not have permission to delete it." });
    }

    private static async Task<IResult> EditIssue(Guid id, IssuePatchRequest issuePatch, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = IssueEndpoints.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            var updatedIssue = await issueService.EditIssueAsync(id, userId, issuePatch);
            return Results.Ok(updatedIssue);
        }
        catch (Exception ex)
        {
            return IssueEndpoints.HandleException(ex);
        }
    }
}
