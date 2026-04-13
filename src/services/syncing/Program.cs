using AspNet.Security.OAuth.GitHub;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Syncing.APIs;
using Syncing.Interfaces;
using Syncing.Services;
using MassTransit;
using ReviewProxy.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
    options.ForwardLimit = null; // Trust all proxies in the chain (Vite -> Gateway -> Syncing)
});

// Named HTTP client for service-to-service calls to IdentityService.
// The X-Internal-Secret header is attached here so SyncingService itself stays stateless.
builder.Services.AddHttpClient("identity", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Identity"] ?? "http://identity");

    var secret = builder.Configuration["Services:InternalSecret"];
    if (!string.IsNullOrWhiteSpace(secret))
        client.DefaultRequestHeaders.Add("X-Internal-Secret", secret);
});

builder.Services.AddHttpClient("github", client =>
{
    client.DefaultRequestHeaders.Add("User-Agent", "ReviewProxy");
    client.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2026-03-10");
});

builder.Services.AddScoped<IGitHubClientFactory, GitHubClientFactory>();
builder.Services.AddScoped<IIdentityClient, IdentityClient>();
builder.Services.AddScoped<IGitHubService, GitHubService>();
builder.Services.AddScoped<ISyncingService, SyncingService>();

// MassTransit
builder.Services.AddMassTransit(options =>
    {
        options.AddConsumer<IssueApprovalConsumer>();

        options.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(builder.Configuration["MassTransit:Host"] ?? "rabbitmq://localhost:5672"), h =>
            {
                h.Username(builder.Configuration["MassTransit:Username"] ?? "guest");
                h.Password(builder.Configuration["MassTransit:Password"] ?? "guest");
            });

            cfg.Message<IssueApprovalEvent>(e => e.SetEntityName("issue-approval-exchange"));

            cfg.ReceiveEndpoint("syncing-service-issue-approval", e =>
            {
                e.ConfigureConsumer<IssueApprovalConsumer>(context);
            });

            cfg.ConfigureEndpoints(context);
        });
    });

// GitHub OAuth owns the full auth flow in this service.
// Cookie holds the OAuth session; after /me is called the JWT is handed off to the client.
builder.Services.AddAuthentication(options =>
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
    options.ClientId = builder.Configuration["GitHub:ClientId"]!;
    options.ClientSecret = builder.Configuration["GitHub:ClientSecret"]!;
    options.Scope.Add("read:user");
    options.CallbackPath = "/api/sync/signin-github";
    options.SaveTokens = true;
});

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseForwardedHeaders();

// Handle PublicUrl override if set to fix Redirect URI issues
var publicUrl = app.Configuration["Services:PublicUrl"];
if (!string.IsNullOrWhiteSpace(publicUrl) && Uri.TryCreate(publicUrl, UriKind.Absolute, out var uri))
{
    app.Use((context, next) =>
    {
        context.Request.Host = new HostString(uri.Host, uri.Port > 0 ? uri.Port : (uri.Scheme == "https" ? 443 : 80));
        context.Request.Scheme = uri.Scheme;
        return next();
    });
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "compose")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks("/health");
app.MapSyncingEndpoints();

app.Run();
