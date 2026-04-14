using Scalar.AspNetCore;
using Syncing.APIs;
using Syncing.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Services via Extensions
builder.Services
    .AddSyncingInfrastructure(builder.Configuration)
    .AddSyncingHttpClients(builder.Configuration)
    .AddSyncingBusinessServices()
    .AddSyncingMessaging(builder.Configuration)
    .AddSyncingAuthentication(builder.Configuration);

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
