using ECommerce.API.DTOs;
using ECommerce.API.Models;

namespace ECommerce.API.Services
{
    public interface ICartService
    {
        // Gets the cart from Redis (creates an empty one if it doesn't exist)
        Task<ShoppingCart> GetCartAsync(string cartId);

        // Saves the cart to Redis with a 30-day expiration
        Task<ShoppingCart> UpdateCartAsync(ShoppingCart cart);

        // Deletes the cart from Redis (used after successful checkout)
        Task<bool> DeleteCartAsync(string cartId);

        // Adds an item, checks SQL for inventory, and saves back to Redis
        Task<ShoppingCart> AddItemToCartAsync(string cartId, AddToCartDto itemDto);
    }
}