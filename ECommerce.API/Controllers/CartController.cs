using Microsoft.AspNetCore.Mvc;
using ECommerce.API.Services;
using ECommerce.API.DTOs;
using ECommerce.API.Models;

namespace ECommerce.API.Controllers
{
    
    [ApiController]
    [Route("api/[controller]")]
    public class CartController(ICartService cartService, ILogger<CartController> logger) : ControllerBase
    {
        /// <summary>
        /// Retrieves the current user's or guest's active shopping cart.
        /// </summary>
        /// <remarks>
        /// If the cart doesn't exist in Redis, it returns a new, empty cart automatically.
        /// </remarks>
        /// <param name="id">The Cart ID (Random string for guests, or User ID for logged-in users).</param>
        /// <returns>The cart object, including a list of items.</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCart(string id)
        {
            var cart = await cartService.GetCartAsync(id);
            return Ok(cart);
        }

        /// <summary>
        /// Adds a specific quantity of a product to the user's cart.
        /// </summary>
        /// <remarks>
        /// **Inventory Validation:** This endpoint will check the master product catalog in real-time. If the requested quantity exceeds the available database stock, it will return a 400 Bad Request.
        /// </remarks>
        /// <param name="id">The Cart ID (Random string for guests, or User ID for logged-in users).</param>
        /// <param name="dto">The Product ID and the requested quantity.</param>
        /// <returns>The fully updated Shopping Cart object.</returns>
        [HttpPost("{id}/add")]
        public async Task<IActionResult> AddToCart(string id, [FromBody] AddToCartDto dto)
        {
            try
            {
                // We pass the string ID to our new Redis-backed service
                var updatedCart = await cartService.AddItemToCartAsync(id, dto);

                // Returning the updated cart object makes React's job much easier
                return Ok(updatedCart);
            }
            catch (InvalidOperationException ex)
            {
                // This catches your custom inventory error and returns a clean 400 Bad Request
                logger.LogWarning("Inventory check failed for Cart {CartId}: {Message}", id, ex.Message);
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                // Catches general errors like "Product not found"
                logger.LogError(ex, "Error adding item to cart {CartId}", id);
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Deletes the cart completely.
        /// </summary>
        /// <remarks>
        /// Typically used after a successful checkout or when a user logs out and abandons a guest cart.
        /// </remarks>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(string id)
        {
            await cartService.DeleteCartAsync(id);
            return Ok(new { message = "Cart successfully cleared." });
        }
    }
}