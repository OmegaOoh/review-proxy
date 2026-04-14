using GitHub;

namespace Syncing.Interfaces.Clients;

public interface IGitHubClientFactory
{
    GitHubClient CreateGitHubClient(string token);
    string GenerateGitHubJwt(string appId, string privateKeyPem);
}
