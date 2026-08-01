using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data;
using SofisCraftShop.Backend.Data.Entities;
using SofisCraftShop.Backend.Services;

namespace SofisCraftShop.Backend.Controllers
{
    public record LoginRequest(string Username);
    public record AuthResponse(string Token, Guid PlayerId, string Username);

    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext db, ITokenService tokenService)
        {
            _db = db;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username))
                return BadRequest("Username is required.");

            // Find or create player
            var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == request.Username);
            if (player == null)
            {
                player = new Player { Username = request.Username };
                _db.Players.Add(player);
                await _db.SaveChangesAsync();
            }

            string token = _tokenService.GenerateJwtToken(player.Id, player.Username);

            return Ok(new AuthResponse(token, player.Id, player.Username));
        }
    }
}
