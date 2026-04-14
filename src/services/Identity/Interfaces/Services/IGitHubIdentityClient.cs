namespace Identity.Interfaces.Services;

public interface IGitHubIdentityClient
{
    Task<List<GitHubUserItem>> SearchUsersAsync(string query);
}

public class GitHubUserItem
{
    public long Id { get; set; }
    public string Login { get; set; } = string.Empty;
    [System.Text.Json.Serialization.JsonPropertyName("avatar_url")]
    public string? AvatarUrl { get; set; }
}
