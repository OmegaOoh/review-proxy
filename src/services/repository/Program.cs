using Scalar.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Repository.Data;
using Repository.Services;
using Repository.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();
builder.Services.AddDbContext<RepoDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("PostgresConnection")));
builder.Services.AddHealthChecks();

// Register services
builder.Services.AddScoped<IRepositoryService, RepositoryService>();
builder.Services.AddScoped<IAuditorService, AuditorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapHealthChecks("/health");
app.MapRepositoryEndpoints();

app.Run();
