using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Services;
using Repository.Interfaces;
using Repository.APIs;

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
builder.Services.AddDbContext<RepoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("RepoDbContext")));
builder.Services.AddHealthChecks();

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
