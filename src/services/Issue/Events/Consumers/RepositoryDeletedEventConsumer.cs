using MassTransit;
using ReviewProxy.Contracts;
using Issue.Data;
using Microsoft.EntityFrameworkCore;

namespace Issue.Events.Consumers;

public class RepositoryDeletedEventConsumer(IssueDbContext dbContext, ILogger<RepositoryDeletedEventConsumer> logger) : IConsumer<RepositoryDeletedEvent>
{
    public async Task Consume(ConsumeContext<RepositoryDeletedEvent> context)
    {
        var repoId = context.Message.RepositoryId;
        logger.LogInformation("Repository {RepoId} deleted, cleaning up issues and repository entries.", repoId);

        // Delete all issues for this repository
        var issues = await dbContext.Issues.Where(i => i.RepositoryId == repoId).ToListAsync();
        if (issues.Any())
        {
            dbContext.Issues.RemoveRange(issues);
        }

        // Delete repository entries
        var repoEntries = await dbContext.Repositories.Where(r => r.Id == repoId).ToListAsync();
        if (repoEntries.Any())
        {
            dbContext.Repositories.RemoveRange(repoEntries);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Cleanup for repository {RepoId} completed. Removed {IssueCount} issues and {RepoCount} repository entries.",
            repoId, issues.Count, repoEntries.Count);
    }
}
