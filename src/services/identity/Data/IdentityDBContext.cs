using Identity.Models;
using Microsoft.EntityFrameworkCore;

namespace Identity.Data
{
    public class IdentityDBContext(DbContextOptions<IdentityDBContext> options) : DbContext(options)
    {
        public DbSet<UserEntry> Repositories => Set<UserEntry>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserEntry>().HasIndex(r => r.GitHubID).IsUnique();
        }
    }
}
