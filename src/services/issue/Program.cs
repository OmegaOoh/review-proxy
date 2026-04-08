using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Issue.Data;
using Issue.Services;
using Issue.Interfaces;
using Issue.APIs;
using MassTransit;

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
builder.Services.AddHttpClient();
builder.Services.AddOpenApi();
builder.Services.AddDbContext<IssueDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("IssueDbContext")));

// MassTransit
builder.Services.AddMassTransit(options =>
    {
        options.AddConsumer<SyncAuditorsEventConsumer>();

        options.UsingRabbitMq((context, cfg) =>
        {
            cfg.Host(new Uri(builder.Configuration["MassTransit:Host"] ?? "rabbitmq://localhost:5672"), h =>
            {
                h.Username(builder.Configuration["MassTransit:Username"] ?? "guest");
                h.Password(builder.Configuration["MassTransit:Password"] ?? "guest");
            });
        });
    });

builder.Services.AddHealthChecks();

builder.Services.AddScoped<IIssueService, IssueService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<IssueDbContext>();
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
app.MapIssueEndpoints();

app.Run();
