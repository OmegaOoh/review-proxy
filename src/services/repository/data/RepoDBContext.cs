using Microsoft.EntityFrameworkCore;
using Repository.Models;

namespace Repository.Data;

public class RepoDbContext(DbContextOptions<RepoDbContext> options) : DbContext(options)
{
    public DbSet<RepositoryEntry> Repositories => Set<RepositoryEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<RepositoryEntry>().HasIndex(r => r.GitHubRepoId).IsUnique();
    }
}
