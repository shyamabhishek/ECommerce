using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using ECommerce.API.Services;
using ECommerce.API.DTOs;

namespace ECommerce.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CartController(ICartService cartService) : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetMyCart()
        {
            var userId = GetUserIdFromToken();
            var cart = await cartService.GetCartAsync(userId);
            return Ok(cart);
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart(AddToCartDto dto)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var cart = await cartService.AddItemToCartAsync(userId, dto);
                return Ok("Item added to cart");
            }
            catch (InvalidOperationException ex)
            {
                // This catches our inventory error and returns a clean 400 Bad Request
                return BadRequest(ex.Message);
            }
        }

        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException("Invalid token.");
            return int.Parse(userIdClaim);
        }
    }
}