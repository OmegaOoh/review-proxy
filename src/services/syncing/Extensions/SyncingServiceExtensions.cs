using Microsoft.AspNetCore.HttpOverrides;
using Syncing.Interfaces;
using Syncing.Services;
using Syncing.Clients;

namespace Syncing.Extensions;

public static class SyncingServiceExtensions
{
    public static IServiceCollection AddSyncingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOpenApi();
        services.AddHealthChecks();

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
            options.ForwardLimit = null;
        });

        return services;
    }

    public static IServiceCollection AddSyncingHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpClient("identity", client =>
        {
            client.BaseAddress = new Uri(configuration["Services:Identity"] ?? "http://identity");
            var secret = configuration["Services:InternalSecret"];
            if (!string.IsNullOrWhiteSpace(secret))
                client.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
        });

        services.AddHttpClient("github", client =>
        {
            client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");
            client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
        });

        return services;
    }

    public static IServiceCollection AddSyncingBusinessServices(this IServiceCollection services)
    {
        services.AddScoped<IGitHubClientFactory, GitHubClientFactory>();
        services.AddScoped<IIdentityClient, IdentityClient>();
        services.AddScoped<IGitHubInstallationService, GitHubInstallationService>();
        services.AddScoped<IGitHubService, GitHubService>();
        services.AddScoped<ISyncingService, SyncingService>();
        return services;
    }
}
