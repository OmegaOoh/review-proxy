namespace Repository.Interfaces.Events;

public interface IRepositoryEventPublisher
{
    Task PublishAuditorListAsync(Guid repoId);
    Task PublishRepositoryDeletedAsync(Guid repoId);
}
