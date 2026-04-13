using Microsoft.EntityFrameworkCore;
using Issue.Data;
using Issue.Interfaces;
using Issue.Models;
using Issue.Models.Dtos;
using MassTransit;
using ReviewProxy.Contracts;

namespace Issue.Services;

public partial class IssueService(IssueDbContext context, IPublishEndpoint publishEndpoint) : IIssueService
{
    private readonly IssueDbContext _context = context;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    private async Task<IssueEntry> GetIssueOrThrowAsync(Guid id) =>
        await _context.Issues.FirstOrDefaultAsync(i => i.Id == id)
        ?? throw new KeyNotFoundException($"Issue with id {id} not found");

    private async Task<RepositoryEntry> GetRepositoryOrThrowAsync(Guid id) =>
        await _context.Repositories.FindAsync(id)
        ?? throw new KeyNotFoundException($"Repository with id {id} not found");

    private async Task<(IssueEntry issue, RepositoryEntry repo, Guid userGuid)> ValidateReviewRequest(Guid id, string userId)
    {
        var issue = await GetIssueOrThrowAsync(id);
        if (issue.Status != IssueStatus.SubmitForReview)
            throw new InvalidOperationException($"Issue with id {id} is not submitted for review");
        if (issue.OwnerId == userId)
            throw new InvalidOperationException($"Issue with id {id} cannot be reviewed by the owner");

        var repo = await GetRepositoryOrThrowAsync(issue.RepositoryId);
        if (!Guid.TryParse(userId, out var userGuid) || !repo.AuditorsId.Contains(userGuid))
            throw new UnauthorizedAccessException($"User {userId} is not an auditor of repository {issue.RepositoryId}");

        return (issue, repo, userGuid);
    }
}
