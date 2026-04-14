using MassTransit;
using Repository.Data;
using Repository.Interfaces.Events;
using ReviewProxy.Contracts;

namespace Repository.Events.Producers;

public class RepositoryEventPublisher(RepoDbContext dbContext, IPublishEndpoint publishEndpoint) : IRepositoryEventPublisher
{
    public async Task PublishAuditorListAsync(Guid repoId)
    {
        var repo = await dbContext.Repositories.FindAsync(repoId);
        if (repo == null) return;

        var auditors = repo.AuditorsIds.Select(Guid.Parse).ToList();
        if (Guid.TryParse(repo.OwnerId, out var ownerId) && !auditors.Contains(ownerId))
        {
            auditors.Add(ownerId);
        }

        await publishEndpoint.Publish(new SyncAuditorListEvent
        {
            RepositoryId = repoId,
            GitHubRepoId = repo.GitHubRepoId,
            Auditors = auditors ?? []
        });
    }

    public async Task PublishRepositoryDeletedAsync(Guid repoId)
    {
        await publishEndpoint.Publish(new RepositoryDeletedEvent
        {
            RepositoryId = repoId
        });
    }
}
