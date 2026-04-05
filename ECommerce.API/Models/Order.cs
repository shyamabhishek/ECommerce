namespace ECommerce.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User? User { get; set; } = null!;
        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }
        public required string OrderStatus { get; set; } = "Pending";
        public List<OrderItem> Items { get; set; } = new();

    }
}
