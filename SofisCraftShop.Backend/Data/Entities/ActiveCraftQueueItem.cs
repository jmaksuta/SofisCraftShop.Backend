using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SofisCraftShop.Backend.Data.Entities
{
    public class ActiveCraftQueueItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid PlayerId { get; set; }
        public Player Player { get; set; } = null!;

        [Required, MaxLength(64)]
        public string RecipeId { get; set; } = string.Empty; // e.g., "rec_berry_juice"

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public int CraftDurationSeconds { get; set; }

        [NotMapped]
        public DateTime CompletesAt => StartedAt.AddSeconds(CraftDurationSeconds);

        [NotMapped]
        public bool IsFinished => DateTime.UtcNow >= CompletesAt;
    }
}
