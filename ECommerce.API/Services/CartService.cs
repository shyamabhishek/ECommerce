using ECommerce.API.Data;
using ECommerce.API.DTOs;
using ECommerce.API.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.API.Services
{
    public class CartService(AppDbContext context) : ICartService
    {
        public async Task<CartDto> GetCartAsync(int userId)
        {
            var cart = await GetOrCreateCartAsync(userId);
            return MapToDto(cart);
        }

        public async Task<CartDto> AddItemToCartAsync(int userId, AddToCartDto itemDto)
        {
            var cart = await GetOrCreateCartAsync(userId);

            var product = await context.Products.FindAsync(itemDto.ProductId)
                ?? throw new Exception("Product not found");

            var existingItem = cart.Items.FirstOrDefault(c => c.ProductId == itemDto.ProductId);

            // --- THE FIX: Inventory Validation ---
            // Calculate what the new quantity would be if we added this
            int currentCartQuantity = existingItem?.Quantity ?? 0;
            int requestedTotalQuantity = currentCartQuantity + itemDto.Quantity;

            // Check if we have enough in the database
            if (requestedTotalQuantity > product.Quantity)
            {
                // Throw a clear error so the controller/middleware knows what went wrong
                throw new InvalidOperationException($"Cannot add item. Only {product.Quantity} in stock.");
            }
            // -------------------------------------

            if (existingItem != null)
            {
                existingItem.Quantity += itemDto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    Quantity = itemDto.Quantity,
                    UnitPrice = product.Price
                });
            }

            await context.SaveChangesAsync();
            return MapToDto(cart);
        }

        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await context.Carts
                .Include(c => c.Items) // FIXED: Using .Items instead of .CartItems
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId };
                context.Carts.Add(cart);
                await context.SaveChangesAsync();
            }

            return cart;
        }

        private CartDto MapToDto(Cart cart)
        {
            return new CartDto
            {
                CartId = cart.Id,
                // FIXED: Using .Items instead of .CartItems
                Items = cart.Items.Select(i => new CartItemDto
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    UnitPrice = i.UnitPrice,
                    Quantity = i.Quantity
                }).ToList()
            };
        }
    }
}