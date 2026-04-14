using Issue.Data;
using Issue.Models;
using MassTransit;
using ReviewProxy.Contracts;

namespace Issue.Events.Consumers;

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
                    GitHubRepoId = eventData.GitHubRepoId,
                    AuditorsId = [.. eventData.Auditors],
                    LastUpdated = eventData.UtcTime,
                    CreatedAt = eventData.UtcTime,
                };
                dbContext.Repositories.Add(repository);
            }
            else
            {
                if (eventData.UtcTime < repository.LastUpdated)
                {
                    logger.LogWarning("Ignored stale message for Repository {RepositoryId}. Event time: {EventTime}, Last data updated: {LastUpdated}",
                                      repoId, eventData.UtcTime, repository.LastUpdated);
                    return; // Return early to avoid processing stale messages
                }
                repository.GitHubRepoId = eventData.GitHubRepoId;
                repository.AuditorsId = [.. eventData.Auditors];
                repository.LastUpdated = eventData.UtcTime;
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
