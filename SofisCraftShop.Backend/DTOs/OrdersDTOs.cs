using SofisCraftShop.Backend.Data.Entities;

namespace SofisCraftShop.Backend.DTOs
{
    public record CustomerOrderDto(
        int Id,
        Guid PlayerId,
        Player Player,
        string CustomerName,
        string RequestedItemId,
        int QuantityRequired,
        int RewardGold,
        int RewardXp,
        bool IsCompleted,
        DateTime ExpirationTimeUtc
    );

    public class OrdersDTOs
    {
    }
}
