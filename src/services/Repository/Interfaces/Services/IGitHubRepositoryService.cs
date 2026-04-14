namespace Repository.Interfaces.Services;

public interface IGitHubRepositoryService
{
    Task<(string FullName, bool HasAccess)> ValidateRepositoryAsync(string githubRepoId, string? githubToken);
}
