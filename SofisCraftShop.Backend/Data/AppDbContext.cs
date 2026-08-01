using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data.Entities;

namespace SofisCraftShop.Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Player> Players => Set<Player>();
        public DbSet<PlayerInventoryItem> PlayerInventoryItems => Set<PlayerInventoryItem>();
        public DbSet<ActiveCraftQueueItem> ActiveCraftQueueItems => Set<ActiveCraftQueueItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure unique items per player
            modelBuilder.Entity<PlayerInventoryItem>()
                .HasIndex(i => new { i.PlayerId, i.ItemId })
                .IsUnique();
        }
    }
}
