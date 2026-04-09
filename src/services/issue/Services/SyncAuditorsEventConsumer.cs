namespace Issue.Services;

using Issue.Data;
using Issue.Models;
using MassTransit;
using Microsoft.Extensions.Logging;
using ReviewProxy.Contracts;

public class SyncAuditorsEventConsumer(
    IssueDbContext dbContext,
    ILogger<SyncAuditorsEventConsumer> logger) : IConsumer<SyncAuditorListEvent>
{
    public async Task Consume(ConsumeContext<SyncAuditorListEvent> context)
    {
        var eventData = context.Message;
        var repoId = eventData.RepositoryId;

        logger.LogInformation("Syncing auditors for Repository {RepositoryId}. MessageId: {MessageId}.",
            repoId, context.MessageId);

        try
        {
            var repository = await dbContext.Repositories.FindAsync(repoId);

            if (repository == null)
            {
                repository = new RepositoryEntry
                {
                    Id = repoId,
                    AuditorsId = [.. eventData.Auditors]
                };
                dbContext.Repositories.Add(repository);
            }
            else
            {
                repository.AuditorsId = [.. eventData.Auditors];
                repository.LastUpdated = DateTime.UtcNow;
            }

            await dbContext.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to sync auditors for Repository {RepositoryId}.", repoId);
            throw; // Re-throw to let MassTransit handle retries
        }
    }
}
