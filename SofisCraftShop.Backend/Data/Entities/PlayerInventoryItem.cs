using System.ComponentModel.DataAnnotations;

namespace SofisCraftShop.Backend.Data.Entities
{
    public class PlayerInventoryItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        [Required, MaxLength(64)]
        public string ItemId { get; set; } = string.Empty; // e.g., "mat_wild_berry"

        [MinLength(0)]
        public int Quantity { get; set; } = 0;
    }
}
