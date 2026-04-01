using System;
using System.ComponentModel.DataAnnotations;

namespace Issue.Models;

public class IssueEntry
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public string Title { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public string Status { get; set; } = "Draft";

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    [Required]
    public string RepositoryId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
