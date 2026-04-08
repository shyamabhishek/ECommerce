namespace ECommerce.API.DTOs
{
    public class ProductDto
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public required string Description { get; set; }
        public string PictureUrl { get; set; }
    }
}