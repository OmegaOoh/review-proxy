using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Issue.Models;

public class IssueEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public IssueStatus Status { get; set; } = IssueStatus.Draft;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    [Required]
    [ForeignKey("Repository")]
    public Guid RepositoryId { get; set; }

    public virtual RepositoryEntry? Repository { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
