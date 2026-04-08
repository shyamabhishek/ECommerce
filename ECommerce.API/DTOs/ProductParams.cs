namespace ECommerce.API.DTOs
{
    public class ProductParams
    {
        private const int MaxPageSize = 50;
        public int PageNumber { get; set; } = 1; //default to page 1
        private int _pageSize = 10; //default to page items per page
        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
        }
        public string? OrderBy { get; set; } // e.g., "priceDesc", "name"

        private string? _searchTerm;
        public string? SearchTerm
        {
            get => _searchTerm;
            // Force the search word to lowercase so "Shirt" and "shirt" match identically
            set => _searchTerm = value?.ToLower();
        }
    }
}
