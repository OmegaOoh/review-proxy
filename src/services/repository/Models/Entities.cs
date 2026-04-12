namespace Repository.Models;

public class RepositoryEntry
{
    public Guid Id { get; set; }
    /// <summary>
    /// The full GitHub repository identifier (e.g., "owner/repo").
    /// </summary>
    public string GitHubRepoId { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<string> AuditorsIds { get; set; } = [];
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
