using GitHub;

namespace Syncing.Interfaces;

public interface IGitHubClientFactory
{
    GitHubClient CreateGitHubClient(string token);
    string GenerateGitHubJwt(string appId, string privateKeyPem);
}
