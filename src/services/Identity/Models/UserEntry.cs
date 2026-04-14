namespace Identity.Models;

public class UserEntry
{
    public Guid Id { get; set; }
    public string GitHubUsername { get; set; } = string.Empty;
    public string? GitHubAvatarUrl { get; set; } = string.Empty;
    public string GitHubID { get; set; } = string.Empty;
}
