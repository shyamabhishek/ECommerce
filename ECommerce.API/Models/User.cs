

namespace ECommerce.API.Models
{
    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        //public required string Password { get; set; }
        public required string PasswordHash { get; set; }
        public string Role { get; set; } = "Customer";
    }
}
