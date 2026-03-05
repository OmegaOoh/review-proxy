using Identity.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace Identity.APIs;

public static class IdentityEndpoints
{
    public static void MapIdentityEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/identities");

        group.MapGet("/signin", ([FromQuery] string? returnUrl = "/") =>
        {
            return Results.Challenge(
                new AuthenticationProperties { RedirectUri = returnUrl },
                new[] { "GitHub" });
        });

        group.MapGet("/signout", ([FromQuery] string? returnUrl = "/") =>
        {
            return Results.SignOut(
                new AuthenticationProperties { RedirectUri = returnUrl },
                new[] { Microsoft.AspNetCore.Authentication.Cookies.CookieAuthenticationDefaults.AuthenticationScheme });
        });

        group.MapGet("/me", async (ClaimsPrincipal user, IIdentityService identityService) =>
        {
            if (user.Identity?.IsAuthenticated != true)
            {
                return Results.Unauthorized();
            }

            var githubId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var username = user.FindFirst(ClaimTypes.Name)?.Value ?? "unknown";
            var avatarUrl = user.FindFirst("urn:github:avatar")?.Value;

            if (githubId == null)
            {
                return Results.Unauthorized();
            }

            var dbUser = await identityService.GetUserByGitHubIdAsync(githubId);
            if (dbUser == null)
            {
                dbUser = await identityService.CreateUserAsync(githubId, username, avatarUrl);
            }
            else
            {
                dbUser = await identityService.UpdateUserAsync(dbUser.Id, username, avatarUrl);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("super_secret_key_that_is_long_enough_for_hmacsha256");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, dbUser.Id.ToString()),
                    new Claim(ClaimTypes.Name, dbUser.GitHubUsername)
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwt = tokenHandler.WriteToken(token);

            return Results.Ok(new { User = dbUser, Token = jwt });
        }).RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { AuthenticationSchemes = "Cookies,Bearer" });
    }
}
