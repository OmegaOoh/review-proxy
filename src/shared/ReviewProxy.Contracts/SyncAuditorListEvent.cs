namespace ReviewProxy.Contracts;

public record SyncAuditorListEvent
{
    public Guid RepositoryId { get; init; }
    public required string GitHubRepoId { get; init; }
    public IEnumerable<Guid> Auditors { get; init; } = [];
    public DateTime UtcTime { get; init; } = DateTime.UtcNow;
}
