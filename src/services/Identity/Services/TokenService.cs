using Identity.Interfaces.Services;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Identity.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    private readonly int jwtExpirationDays = 7;

    public string IssueJwt(Guid userId, string username)
    {
        var key = Encoding.UTF8.GetBytes(
            configuration["Jwt:Key"] ?? "super_secret_key_that_is_long_enough_for_hmacsha256");

        var tokenHandler = new JwtSecurityTokenHandler();

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(ClaimTypes.Name, username)
            ]),
            Expires = DateTime.UtcNow.AddDays(jwtExpirationDays),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}
