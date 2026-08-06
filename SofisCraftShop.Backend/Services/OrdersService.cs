using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data;
using SofisCraftShop.Backend.Data.Entities;
using SofisCraftShop.Backend.DTOs;
using System.Collections.ObjectModel;
using System.Security.Claims;

namespace SofisCraftShop.Backend.Services
{
    public interface IOrdersService
    {
        Task<List<CustomerOrderDto>> GetOrders(Guid playerId);
        Task<IActionResult> FulfillOrder(Guid playerId, int orderId);
    }


    public class OrdersService : IOrdersService
    {

        private readonly AppDbContext _db;

        public OrdersService(AppDbContext db) => _db = db;

        public async Task<IActionResult> FulfillOrder(Guid playerId, int orderId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<CustomerOrderDto>> GetOrders(Guid playerId)
        {
            //var player = _db.Players.FirstOrDefault(p => p.Id == playerId) ?? throw new KeyNotFoundException("Player not found.");

            //List<CustomerOrderDto> result = player.CustomerOrders.Select(co => new CustomerOrderDto(
            //    co.Id, co.PlayerId, co.Player, co.CustomerName, co.RequestedItemId, co.QuantityRequired, 
            //    co.RewardGold, co.RewardXp, co.IsCompleted, co.ExpirationTimeUtc)).ToList();

            //return result;
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await _db.Players.FirstOrDefaultAsync<Player>(p => p.Id == CurrentPlayerId);
            if (player == null) return null;// NotFound("Player not found");

            // Check if player has fewer than 3 orders; generate new ones if needed
            var activeOrders = await _db.CustomerOrders
                .Where(o => o.PlayerId == player.Id && !o.IsCompleted)
                .ToListAsync();

            if (activeOrders.Count < 3)
            {
                // Generate a quick random order (e.g., Wood Logs or Copper Daggers)
                var newOrder = new CustomerOrder
                {
                    PlayerId = player.Id,
                    CustomerName = "Town Villager",
                    RequestedItemId = "wood_log",
                    QuantityRequired = 3,
                    RewardGold = 45,
                    RewardXp = 15,
                    ExpirationTimeUtc = DateTime.UtcNow.AddHours(2)
                };
                _db.CustomerOrders.Add(newOrder);
                await _db.SaveChangesAsync();
                activeOrders.Add(newOrder);
            }
        }
    }
}
