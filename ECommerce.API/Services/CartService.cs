using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace ECommerce.API.Services
{
    // We inject IDistributedCache (Redis) and AppDbContext (SQL Database)
    public class CartService(IDistributedCache cache, AppDbContext context) : ICartService
    {
        public async Task<ShoppingCart> GetCartAsync(string cartId)
        {
            var data = await cache.GetStringAsync(cartId);

            // If the cart doesn't exist in Redis yet, return a new empty one
            if (string.IsNullOrEmpty(data))
            {
                return new ShoppingCart(cartId);
            }

            // Convert the JSON string back into a ShoppingCart object
            return JsonSerializer.Deserialize<ShoppingCart>(data)!;
        }

        public async Task<ShoppingCart> UpdateCartAsync(ShoppingCart cart)
        {
            // Give the cart a 30-day lifespan. Redis cleans it up automatically!
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30)
            };

            var jsonData = JsonSerializer.Serialize(cart);
            await cache.SetStringAsync(cart.Id, jsonData, options);

            return cart;
        }

        public async Task<bool> DeleteCartAsync(string cartId)
        {
            await cache.RemoveAsync(cartId);
            return true;
        }

        public async Task<ShoppingCart> AddItemToCartAsync(string cartId, AddToCartDto itemDto)
        {
            // 1. Get the current cart from Redis
            var cart = await GetCartAsync(cartId);

            // 2. Look up the product in SQL to check stock and get details
            var product = await context.Products.FindAsync(itemDto.ProductId)
                ?? throw new Exception("Product not found");

            var existingItem = cart.Items.FirstOrDefault(c => c.ProductId == itemDto.ProductId);

            // 3. Inventory Validation
            int currentCartQuantity = existingItem?.Quantity ?? 0;
            int requestedTotalQuantity = currentCartQuantity + itemDto.Quantity;

            if (requestedTotalQuantity > product.Quantity)
            {
                throw new InvalidOperationException($"Cannot add item. Only {product.Quantity} in stock.");
            }

            // 4. Update the cart object
            if (existingItem != null)
            {
                // If it's already in the cart, just increase the quantity
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                // If it's a new item, add it to the list
                cart.Items.Add(new ShoppingCartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = itemDto.Quantity,
                    PictureUrl = product.PictureUrl ?? "" // Ensures the image shows up in the React cart!
                });
            }

            // 5. Save the updated cart back to Redis
            return await UpdateCartAsync(cart);
        }
    }
}