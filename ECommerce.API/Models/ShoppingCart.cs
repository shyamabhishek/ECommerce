namespace ECommerce.API.Models
{
    public class ShoppingCart
    {
        //this is the radis key
        public string Id { get; set; }
        // RIGHT
        public List<ShoppingCartItem> Items { get; set; } = new List<ShoppingCartItem>();
        //Empty constructor for JSON deserialization
        public ShoppingCart() { }
        public ShoppingCart(string id)
        {
            Id = id;
        }

    }
    public class ShoppingCartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string PictureUrl { get; set; } 
    }
}
