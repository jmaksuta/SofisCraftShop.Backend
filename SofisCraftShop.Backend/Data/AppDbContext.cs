using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data.Entities;

namespace SofisCraftShop.Backend.Data
{
    public class AppDbContext : IdentityDbContext<IdentityUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<PlayerInventoryItem> PlayerInventoryItems => Set<PlayerInventoryItem>();
        public DbSet<ActiveCraftQueueItem> ActiveCraftQueueItems => Set<ActiveCraftQueueItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure custom entity constraints
            modelBuilder.Entity<PlayerInventoryItem>()
                .HasIndex(i => new { i.PlayerId, i.ItemId })
                .IsUnique();

            // Link Player record to IdentityUser via 1-to-1 or explicit Foreign Key
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Username)
                .IsUnique();
        }
    }
}
