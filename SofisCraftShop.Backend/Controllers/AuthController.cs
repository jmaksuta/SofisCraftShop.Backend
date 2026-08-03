using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SofisCraftShop.Backend.Data;
using SofisCraftShop.Backend.Data.Entities;
using SofisCraftShop.Backend.Services;
using System;
using System.Threading.Tasks;
using static SofisCraftShop.Backend.DTOs.AuthDTOs;

namespace SofisCraftShop.Backend.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {

        private readonly AppDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthController(AppDbContext db, UserManager<IdentityUser> userManager, SignInManager<IdentityUser> signInManager, ITokenService tokenService)
        {
            _db = db;
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest(new { message = "Username and Password are required." });

            var existingUser = await _userManager.FindByNameAsync(request.Username);
            if (existingUser != null)
                return BadRequest(new { message = "Username is already taken." });

            var user = new IdentityUser
            {
                UserName = request.Username,
                Email = request.Email
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return BadRequest(new { message = errors });
            }

            // Automatically sign in / issue token after registration
            string token = _tokenService.GenerateJwtToken(Guid.Parse(user.Id), user.UserName);
            return Ok(new AuthResponse(token, user.Id, user.UserName, user.Email ?? ""));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                return Unauthorized(new { message = "Invalid username or password." });

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid username or password." });

            string token = _tokenService.GenerateJwtToken(Guid.Parse(user.Id), user.UserName!);
            return Ok(new AuthResponse(token, user.Id, user.UserName!, user.Email ?? ""));

            //if (string.IsNullOrWhiteSpace(request.Username))
            //    return BadRequest("Username is required.");

            //// Find or create player
            //var player = await _db.Players.FirstOrDefaultAsync(p => p.Username == request.Username);
            //if (player == null)
            //{
            //    player = new Player { Username = request.Username };
            //    _db.Players.Add(player);
            //    await _db.SaveChangesAsync();
            //}

            //string token = _tokenService.GenerateJwtToken(player.Id, player.Username);

            //return Ok(new AuthResponse(token, player.Id, player.Username));
        }
    }
}
