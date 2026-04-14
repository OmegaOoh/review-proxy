using System.Security.Claims;

namespace Issue.APIs;

public static class IssueEndpoints
{
    public static IEndpointRouteBuilder MapIssueEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/issues").RequireAuthorization();

        group.MapManagementEndpoints();
        group.MapReviewEndpoints();

        return app;
    }

    internal static string? GetUserId(ClaimsPrincipal user) =>
        user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;

    internal static IResult HandleException(Exception ex) => ex switch
    {
        KeyNotFoundException => Results.NotFound(new { message = ex.Message }),
        InvalidOperationException => Results.BadRequest(new { message = ex.Message }),
        UnauthorizedAccessException => Results.Forbid(),
        _ => Results.StatusCode(500)
    };
}
