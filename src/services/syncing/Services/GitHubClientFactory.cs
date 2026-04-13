using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using Syncing.Interfaces;

namespace Syncing.Services;

public class GitHubClientFactory(IHttpClientFactory httpClientFactory, ILogger<GitHubClientFactory> logger) : IGitHubClientFactory
{
    public GitHubClient CreateGitHubClient(string token)
    {
        var httpClient = httpClientFactory.CreateClient("github");
        var tokenProvider = new TokenProvider(token);
        var adapter = RequestAdapter.Create(new TokenAuthProvider(tokenProvider), httpClient);
        return new GitHubClient(adapter);
    }

    public string GenerateGitHubJwt(string appId, string privateKeyPem)
    {
        using var rsa = RSA.Create();
        try { rsa.ImportFromPem(privateKeyPem.Replace("\\n", "\n").Trim().ToCharArray()); }
        catch (Exception ex) { logger.LogError(ex, "RSA import failed. Check PrivateKeyPath."); throw; }

        var handler = new JwtSecurityTokenHandler();
        var now = DateTimeOffset.UtcNow;
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = appId,
            IssuedAt = now.AddSeconds(-60).UtcDateTime,
            Expires = now.AddMinutes(9).UtcDateTime,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
