using System.Security.Cryptography;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using GitHub;
using GitHub.Octokit.Client;
using GitHub.Octokit.Client.Authentication;
using Syncing.Interfaces;

namespace Syncing.Clients;

public class GitHubClientFactory(IHttpClientFactory httpClientFactory, ILogger<GitHubClientFactory> logger) : IGitHubClientFactory
{
    private const int JwtClockSkewSeconds = -60;
    private const int JwtExpirationMinutes = 9;

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
            IssuedAt = now.AddSeconds(JwtClockSkewSeconds).UtcDateTime,
            Expires = now.AddMinutes(JwtExpirationMinutes).UtcDateTime,
            SigningCredentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
        };

        return handler.WriteToken(handler.CreateToken(descriptor));
    }
}
