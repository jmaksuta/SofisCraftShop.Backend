using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data;
using SofisCraftShop.Backend.Data.Entities;
using SofisCraftShop.Backend.DTOs;
using SofisCraftShop.Backend.Services;
using System.Security.Claims;

namespace SofisCraftShop.Backend.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        //private readonly AppDbContext _context;

        private readonly IOrdersService _ordersService;

        public OrdersController(IOrdersService ordersService)
        {
            _ordersService = ordersService;
        }

        private Guid CurrentPlayerId
        {
            get
            {
                var claim = User.FindFirst(ClaimTypes.NameIdentifier);
                return claim != null ? Guid.Parse(claim.Value) : Guid.Empty;
            }
        }


        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await _db.Players.FirstOrDefaultAsync<Player>(p => p.Id == CurrentPlayerId);
            if (player == null) return NotFound("Player not found");

            List<CustomerOrderDto> result = await _ordersService.GetOrders(CurrentPlayerId);
            return Ok(result);

            

            // Check if player has fewer than 3 orders; generate new ones if needed
            var activeOrders = await _context.CustomerOrders
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
                _context.CustomerOrders.Add(newOrder);
                await _context.SaveChangesAsync();
                activeOrders.Add(newOrder);
            }

            return Ok(activeOrders);
        }

        [HttpPost("fulfill/{orderId}")]
        public async Task<IActionResult> FulfillOrder(int orderId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var player = await _context.Players
                .Include(p => p.InventoryItems)
                .FirstOrDefaultAsync(p => p.IdentityUserId == userId);

            if (player == null) return NotFound();

            var order = await _context.CustomerOrders.FirstOrDefaultAsync(o => o.Id == orderId && o.PlayerId == player.Id);
            if (order == null || order.IsCompleted) return BadRequest("Invalid or completed order.");

            // Verify player has the requested items
            var invItem = player.InventoryItems.FirstOrDefault(i => i.ItemId == order.RequestedItemId);
            if (invItem == null || invItem.Quantity < order.QuantityRequired)
            {
                return BadRequest("Not enough items in inventory to fulfill order.");
            }

            // Deduct items and award rewards
            invItem.Quantity -= order.QuantityRequired;
            if (invItem.Quantity <= 0)
            {
                _context.InventoryItems.Remove(invItem);
            }

            player.Gold += order.RewardGold;
            order.IsCompleted = true;

            await _context.SaveChangesAsync();

            return Ok(new { success = true, newGold = player.Gold, message = "Order fulfilled!" });
        }
    }
}
