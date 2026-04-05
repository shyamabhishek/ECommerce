namespace ECommerce.API.DTOs
{
    public class RegisterDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string Role { get; set; } = "Customer";
    }
}
