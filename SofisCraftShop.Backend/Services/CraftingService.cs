using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data;
using SofisCraftShop.Backend.Data.Entities;
using SofisCraftShop.Backend.DTOs;

namespace SofisCraftShop.Backend.Services
{
    public interface ICraftingService
    {
        Task<PlayerSyncResponse> GetPlayerSyncDataAsync(Guid playerId);
        Task<CraftQueueItemDto> StartCraftAsync(Guid playerId, string recipeId);
        Task<PlayerSyncResponse> ClaimCompletedCraftAsync(Guid playerId, Guid queueItemId);
    }

    public class CraftingService : ICraftingService
    {
        private readonly AppDbContext _db;

        // Static recipe lookup dictionary (In production, load from DB/Redis)
        private static readonly Dictionary<string, (List<(string item, int qty)> ingredients, int gold, int duration, string resultItem, int resultQty)> RecipeCatalog = new()
        {
            { "rec_berry_juice", (newList: new() { ("mat_wild_berry", 2), ("mat_clean_water", 1) }, gold: 10, duration: 5, resultItem: "item_berry_juice", resultQty: 1) }
        };

        public CraftingService(AppDbContext db) => _db = db;

        public async Task<PlayerSyncResponse> GetPlayerSyncDataAsync(Guid playerId)
        {
            var player = await _db.Players
                .Include(p => p.Inventory)
                .Include(p => p.CraftQueue)
                .FirstOrDefaultAsync(p => p.Id == playerId)
                ?? throw new KeyNotFoundException("Player not found.");

            var inventoryDict = player.Inventory.ToDictionary(i => i.ItemId, i => i.Quantity);
            var now = DateTime.UtcNow;

            var queueDtos = player.CraftQueue.Select(q => new CraftQueueItemDto(
                q.Id,
                q.RecipeId,
                q.StartedAt,
                q.CraftDurationSeconds,
                Math.Max(0, (q.CompletesAt - now).TotalSeconds),
                now >= q.CompletesAt
            )).ToList();

            return new PlayerSyncResponse(player.Id, player.Gold, inventoryDict, queueDtos);
        }

        public async Task<CraftQueueItemDto> StartCraftAsync(Guid playerId, string recipeId)
        {
            if (!RecipeCatalog.TryGetValue(recipeId, out var recipe))
                throw new ArgumentException("Invalid recipe ID.");

            var player = await _db.Players
                .Include(p => p.Inventory)
                .Include(p => p.CraftQueue)
                .FirstOrDefaultAsync(p => p.Id == playerId)
                ?? throw new KeyNotFoundException("Player not found.");

            // 1. Queue Slot Check
            if (player.CraftQueue.Count >= 3)
                throw new InvalidOperationException("Crafting queue is full.");

            // 2. Gold Check
            if (player.Gold < recipe.gold)
                throw new InvalidOperationException("Insufficient gold.");

            // 3. Ingredient Verification
            foreach (var (itemId, requiredQty) in recipe.ingredients)
            {
                var invItem = player.Inventory.FirstOrDefault(i => i.ItemId == itemId);
                if (invItem == null || invItem.Quantity < requiredQty)
                    throw new InvalidOperationException($"Missing ingredient: {itemId}");
            }

            // --- ATOMIC STATE MUTATION ---
            player.Gold -= recipe.gold;

            foreach (var (itemId, requiredQty) in recipe.ingredients)
            {
                var invItem = player.Inventory.First(i => i.ItemId == itemId);
                invItem.Quantity -= requiredQty;
                if (invItem.Quantity <= 0) _db.PlayerInventoryItems.Remove(invItem);
            }

            var queueItem = new ActiveCraftQueueItem
            {
                PlayerId = playerId,
                RecipeId = recipeId,
                StartedAt = DateTime.UtcNow,
                CraftDurationSeconds = recipe.duration
            };

            _db.ActiveCraftQueueItems.Add(queueItem);
            await _db.SaveChangesAsync();

            return new CraftQueueItemDto(
                queueItem.Id,
                queueItem.RecipeId,
                queueItem.StartedAt,
                queueItem.CraftDurationSeconds,
                recipe.duration,
                false
            );
        }

        public async Task<PlayerSyncResponse> ClaimCompletedCraftAsync(Guid playerId, Guid queueItemId)
        {
            var player = await _db.Players
                .Include(p => p.Inventory)
                .Include(p => p.CraftQueue)
                .FirstOrDefaultAsync(p => p.Id == playerId)
                ?? throw new KeyNotFoundException("Player not found.");

            var queueItem = player.CraftQueue.FirstOrDefault(q => q.Id == queueItemId)
                ?? throw new KeyNotFoundException("Craft task not found in active queue.");

            // 4. Server-Side Timer Validation
            if (DateTime.UtcNow < queueItem.CompletesAt)
            {
                var remaining = (queueItem.CompletesAt - DateTime.UtcNow).TotalSeconds;
                throw new InvalidOperationException($"Crafting not complete yet. {remaining:F1}s remaining.");
            }

            var recipe = RecipeCatalog[queueItem.RecipeId];

            // Reward result item to player inventory
            var invItem = player.Inventory.FirstOrDefault(i => i.ItemId == recipe.resultItem);
            if (invItem != null)
            {
                invItem.Quantity += recipe.resultQty;
            }
            else
            {
                _db.PlayerInventoryItems.Add(new PlayerInventoryItem
                {
                    PlayerId = playerId,
                    ItemId = recipe.resultItem,
                    Quantity = recipe.resultQty
                });
            }

            // Remove task from queue
            _db.ActiveCraftQueueItems.Remove(queueItem);
            await _db.SaveChangesAsync();

            return await GetPlayerSyncDataAsync(playerId);
        }
    }
}
