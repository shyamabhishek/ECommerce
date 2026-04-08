using ECommerce.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ECommerce.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrderController(IOrderService orderService) : ControllerBase
    {
        /// <summary>
        /// Processes a checkout and converts the user's cart into a finalized order.
        /// </summary>
        /// <remarks>
        /// **Transactional Process:**
        /// 1. Verifies final inventory levels one last time to prevent over-selling.
        /// 2. Deducts the purchased quantities from the master product catalog.
        /// 3. Generates a final order receipt locking in the purchase prices.
        /// 4. Completely empties the user's active cart.
        /// </remarks>
        /// <returns>The finalized Order receipt showing the total amount and order status.</returns>
        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout()
        {
            try
            {
                var userId = GetUserIdFromToken();
                //this triggers the massive transaction we built in the service layer
                var order = await orderService.CheckoutAsync(userId);
                return Ok(order);// returns the final receipt with order details
            }
            catch (InvalidOperationException ex)
            {
                // This catches our inventory error and returns a clean 400 Bad Request
                return BadRequest(ex.Message);
            }
        }

        /// <summary>
        /// Retrieves the complete order history for the currently logged-in user.
        /// </summary>
        /// <remarks>
        /// Returns all past purchases made by this user, automatically sorted by date (newest first).
        /// </remarks>
        /// <returns>A list of past Order DTOs.</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetOrderHistory()
        {
            var userId = GetUserIdFromToken();
            var orders = await orderService.GetUserOrdersAsync(userId);
            return Ok(orders);
        }
        //helper to securely extract the5 logged in user ID
        private int GetUserIdFromToken()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdClaim)) throw new UnauthorizedAccessException("Invalid token");
            return int.Parse(userIdClaim);
        }
    }
}
