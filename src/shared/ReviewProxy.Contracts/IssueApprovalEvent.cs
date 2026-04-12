namespace ReviewProxy.Contracts;

public record IssueApprovalEvent
{
    public Guid IssueId { get; init; }
    public Guid ApproverId { get; init; }
    public required string Title { get; init; }
    public string? Body { get; init; }
    public required string IssueOwner { get; init; }
    public DateTime UtcTime { get; init; } = DateTime.UtcNow;
}
