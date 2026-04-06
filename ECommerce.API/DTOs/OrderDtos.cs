namespace ECommerce.API.DTOs
{
    public class OrderDtos
    {
        public int Id { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public required string OrderStatus { get; set; }
        public List<OrderItemDtos> Items { get; set; } = new();
    }
    public class OrderItemDtos
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
