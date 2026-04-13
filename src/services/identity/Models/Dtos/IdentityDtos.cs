namespace Identity.Models.Dtos;

public record ExchangeRequest(string GitHubId, string Username, string? AvatarUrl);
public record EnsureRequest(string GitHubId, string Username, string? AvatarUrl);
