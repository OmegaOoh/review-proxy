using MassTransit;
using Repository.Data;
using Repository.Interfaces;
using ReviewProxy.Contracts;

namespace Repository.Services;

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
            Auditors = auditors ?? []
        });
    }
}
