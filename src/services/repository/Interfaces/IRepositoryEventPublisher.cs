namespace Repository.Interfaces;

public interface IRepositoryEventPublisher
{
    Task PublishAuditorListAsync(Guid repoId);
    Task PublishRepositoryDeletedAsync(Guid repoId);
}
