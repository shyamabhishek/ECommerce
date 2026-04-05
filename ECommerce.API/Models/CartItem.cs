namespace ECommerce.API.Models
{
    public class CartItem
    {
        public int Id { get; set; }
        public int CartId { get; set; }
        public Cart? Cart { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; }
        //we store the price at the time they added it
        public decimal UnitPrice { get; set; }
    }
}
