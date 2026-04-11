using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Services;
using Repository.Interfaces;
using Repository.APIs;
using MassTransit;
using ReviewProxy.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer("Bearer", options =>
    {
        var key = System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "super_secret_key_that_is_long_enough_for_hmacsha256");
        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key)
        };
    });
builder.Services.AddAuthorization();

// Named HTTP client for service-to-service calls to IdentityService.
builder.Services.AddHttpClient("identity", client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:Identity"] ?? "http://identity");
});

builder.Services.AddOpenApi();
builder.Services.AddDbContext<RepoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RepoDbContext")));

// MassTransit
builder.Services.AddMassTransit(options =>
    {
        options.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(builder.Configuration["MassTransit:Host"] ?? "rabbitmq://localhost:5672"), h =>
            {
                h.Username(builder.Configuration["MassTransit:Username"] ?? "guest");
                h.Password(builder.Configuration["MassTransit:Password"] ?? "guest");
            });

            cfg.Message<SyncAuditorListEvent>(e => e.SetEntityName("auditor-sync-exchange"));
        });
    });

builder.Services.AddHealthChecks();

builder.Services.AddScoped<IRepositoryEventPublisher, RepositoryEventPublisher>();
builder.Services.AddScoped<IRepositoryService, RepositoryService>();
builder.Services.AddScoped<IAuditorService, AuditorService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<RepoDbContext>();
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "compose")
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.UseAuthentication();
app.UseAuthorization();
app.MapRepositoryEndpoints();

app.Run();
