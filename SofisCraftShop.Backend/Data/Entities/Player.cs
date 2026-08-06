using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SofisCraftShop.Backend.Data.Entities
{
    public class Player
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required, MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        public int Level { get; set; } = 1;
        public long Gold { get; set; } = 100;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastSyncedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public ICollection<PlayerInventoryItem> Inventory { get; set; } = new List<PlayerInventoryItem>();
        public ICollection<ActiveCraftQueueItem> CraftQueue { get; set; } = new List<ActiveCraftQueueItem>();

        public ICollection<CustomerOrder> CustomerOrders { get; set; } = new List<CustomerOrder>();
    }
}
