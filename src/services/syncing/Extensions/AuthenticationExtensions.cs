using Microsoft.AspNetCore.Authentication.Cookies;
using AspNet.Security.OAuth.GitHub;

namespace Syncing.Extensions;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddSyncingAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = GitHubAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
        {
            options.Cookie.Name = "AppAuthSession";
            options.Cookie.SameSite = SameSiteMode.Lax;
        })
        .AddGitHub(options =>
        {
            options.ClientId = configuration["GitHub:ClientId"]!;
            options.ClientSecret = configuration["GitHub:ClientSecret"]!;
            options.Scope.Add("read:user");
            options.CallbackPath = "/api/sync/signin-github";
            options.SaveTokens = true;
        });

        services.AddAuthorization();
        return services;
    }
}
