using Microsoft.EntityFrameworkCore;
using Issue.Models;

namespace Issue.Data;

public class IssueDbContext : DbContext
{
    public IssueDbContext(DbContextOptions<IssueDbContext> options)
        : base(options)
    {
    }

    public DbSet<IssueEntry> Issues { get; set; }
    public DbSet<RepositoryEntry> Repositories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IssueEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired();
            entity.Property(e => e.OwnerId).IsRequired();
            entity.Property(e => e.RepositoryId).IsRequired();
            entity.Property(e => e.Status)
                  .HasConversion<string>();
        });

        modelBuilder.Entity<RepositoryEntry>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AuditorsId).IsRequired();
        });
    }
}
