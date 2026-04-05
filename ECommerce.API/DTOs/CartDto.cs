namespace ECommerce.API.DTOs
{
    public class  AddToCartDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }

    }
    public class CartItemDto
    {
        public int ProductId { get; set; }
        public required string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
    public class CartDto
    {
        public int CartId { get; set; }
        public List<CartItemDto> Items { get; set; } = new();
        public decimal CartTotal => Items.Sum(item => item.TotalPrice);
    }
}
