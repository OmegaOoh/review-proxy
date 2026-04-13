using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Issue.Interfaces;
using Issue.Models.Dtos;

namespace Issue.APIs;

internal static class IssueReviewEndpoints
{
    public static void MapReviewEndpoints(this IEndpointRouteBuilder group)
    {
        group.MapPost("/{id:guid}/approve", ApproveIssue).WithName("ApproveIssue");
        group.MapPost("/{id:guid}/reject", RejectIssue).WithName("RejectIssue");
    }

    private static async Task<IResult> ApproveIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = IssueEndpoints.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            await issueService.ApproveIssueAsync(id, userId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return IssueEndpoints.HandleException(ex);
        }
    }

    private static async Task<IResult> RejectIssue(Guid id, IIssueService issueService, ClaimsPrincipal user)
    {
        var userId = IssueEndpoints.GetUserId(user);
        if (string.IsNullOrEmpty(userId)) return Results.Unauthorized();

        try
        {
            await issueService.RejectIssueAsync(id, userId);
            return Results.NoContent();
        }
        catch (Exception ex)
        {
            return IssueEndpoints.HandleException(ex);
        }
    }
}
