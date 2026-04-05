using ECommerce.API.DTOs;

namespace ECommerce.API.Services
{
    public interface ICartService
    {
        Task<CartDto> GetCartAsync(int userId);
        Task<CartDto> AddItemToCartAsync(int userId, AddToCartDto itemDto);
    }
}