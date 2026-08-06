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

        public DbSet<CustomerOrder> CustomerOrders => Set<CustomerOrder>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            PlayerInventoryItemConfig(modelBuilder);

            PlayerConfig(modelBuilder);

            CustomerOrderConfig(modelBuilder);

        }

        private void PlayerInventoryItemConfig(ModelBuilder modelBuilder)
        {
            // Configure custom entity constraints
            modelBuilder.Entity<PlayerInventoryItem>()
                .HasIndex(i => new { i.PlayerId, i.ItemId })
                .IsUnique();
        }

        private void PlayerConfig(ModelBuilder modelBuilder)
        {
            // Link Player record to IdentityUser via 1-to-1 or explicit Foreign Key
            modelBuilder.Entity<Player>()
                .HasIndex(p => p.Username)
                .IsUnique();
        }

        private void CustomerOrderConfig(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<CustomerOrder>(entity =>
            {
                // Table name mapping
                entity.ToTable("CustomerOrders");

                // Primary Key
                entity.HasKey(o => o.Id);

                // Property constraints
                entity.Property(o => o.CustomerName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasDefaultValue("Townsperson");

                entity.Property(o => o.RequestedItemId)
                    .IsRequired()
                    .HasMaxLength(64);

                entity.Property(o => o.QuantityRequired)
                    .HasDefaultValue(1);

                entity.Property(o => o.RewardGold)
                    .HasDefaultValue(0);

                entity.Property(o => o.RewardXp)
                    .HasDefaultValue(0);

                entity.Property(o => o.IsCompleted)
                    .HasDefaultValue(false);

                entity.Property(o => o.ExpirationTimeUtc)
                    .IsRequired();

                // Relationship: Player (1) <---> CustomerOrders (Many)
                entity.HasOne(o => o.Player)
                    .WithMany(p => p.CustomerOrders) 
                    .HasForeignKey(o => o.PlayerId)
                    .OnDelete(DeleteBehavior.Cascade); // Deleting a player cleans up their active orders

                // Indexing for faster query performance when fetching active orders
                entity.HasIndex(o => new { o.PlayerId, o.IsCompleted });
            });
        }
    }
}
