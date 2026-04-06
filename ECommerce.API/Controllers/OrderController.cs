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


        [HttpGet]
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
