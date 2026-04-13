namespace Repository.Interfaces;

public interface IGitHubRepositoryService
{
    Task<(string FullName, bool HasAccess)> ValidateRepositoryAsync(string githubRepoId, string? githubToken);
}
