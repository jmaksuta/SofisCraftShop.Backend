namespace SofisCraftShop.Backend.DTOs
{
    public record StartCraftRequest(string RecipeId);

    public record ClaimCraftRequest(Guid QueueItemId);

    public record PlayerSyncResponse(
        Guid PlayerId,
        long Gold,
        Dictionary<string, int> Inventory,
        List<CraftQueueItemDto> ActiveQueue
    );

    public record CraftQueueItemDto(
        Guid QueueItemId,
        string RecipeId,
        DateTime StartedAt,
        int CraftDurationSeconds,
        double RemainingSeconds,
        bool IsReadyToClaim
    );

    public class CraftingDTOs
    {
    }
}
