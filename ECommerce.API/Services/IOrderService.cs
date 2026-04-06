using ECommerce.API.DTOs;
namespace ECommerce.API.Services
{
    public interface IOrderService
    {
        Task<OrderDtos> CheckoutAsync(int userId);
        Task<List<OrderDtos>> GetUserOrdersAsync(int userId);
    }
}
