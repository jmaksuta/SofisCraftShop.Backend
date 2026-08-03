namespace SofisCraftShop.Backend.DTOs
{
    public class AuthDTOs
    {
        public record RegisterRequest(string Username, string Email, string Password);
        public record LoginRequest(string Username, string Password);
        public record AuthResponse(string Token, string PlayerId, string Username, string Email);

    }
}
