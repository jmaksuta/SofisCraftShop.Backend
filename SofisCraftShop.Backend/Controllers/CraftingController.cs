using Microsoft.AspNetCore.Mvc;
using SofisCraftShop.Backend.DTOs;
using SofisCraftShop.Backend.Services;

namespace SofisCraftShop.Backend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class CraftingController : ControllerBase
    {
        private readonly ICraftingService _craftingService;

        public CraftingController(ICraftingService craftingService)
        {
            _craftingService = craftingService;
        }

        /// <summary>
        /// Get full player inventory, gold, and active craft timers.
        /// </summary>
        [HttpGet("sync/{playerId:guid}")]
        public async Task<IActionResult> GetSyncData(Guid playerId)
        {
            var response = await _craftingService.GetPlayerSyncDataAsync(playerId);
            return Ok(response);
        }

        /// <summary>
        /// Request to start crafting an item. Deducts items upfront.
        /// </summary>
        [HttpPost("start/{playerId:guid}")]
        public async Task<IActionResult> StartCraft(Guid playerId, [FromBody] StartCraftRequest request)
        {
            try
            {
                var result = await _craftingService.StartCraftAsync(playerId, request.RecipeId);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Claim a finished craft task. Validates server time before granting rewards.
        /// </summary>
        [HttpPost("claim/{playerId:guid}")]
        public async Task<IActionResult> ClaimCraft(Guid playerId, [FromBody] ClaimCraftRequest request)
        {
            try
            {
                var updatedSync = await _craftingService.ClaimCompletedCraftAsync(playerId, request.QueueItemId);
                return Ok(updatedSync);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
