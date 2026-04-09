using System.ComponentModel.DataAnnotations;
namespace Issue.Models;

public class RepositoryEntry
{
    [Key]
    public Guid Id { get; set; }

    public Guid[] AuditorsId { get; set; } = [];
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
