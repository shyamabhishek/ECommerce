
using ECommerce.API.Data;
using ECommerce.API.DTOs;
using Microsoft.EntityFrameworkCore;
using ECommerce.API.Models;

namespace ECommerce.API.Services
{
    public class OrderService(AppDbContext context) : IOrderService
    {
        public async Task<OrderDtos> CheckoutAsync (int userId)
        {
            //get the cart with items and products for the user
            var cart =  await context.Carts
                .Include(c=> c.Items)
                .ThenInclude(i=> i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId)
            ?? throw new InvalidOperationException("Cart not found");

            if(!cart.Items.Any())
             throw new InvalidOperationException("Cart is empty");
            //calculate total and create the base order
            var order = new Order
            {
                UserId = userId,
                OrderDate = DateTime.UtcNow,
                OrderStatus = "Pending",
                TotalAmount = cart.Items.Sum(i => i.Quantity * i.Product.Price),
            };
            //move items from cart to order and update inventory
            foreach (var cartItem in cart.Items)
            {
                    //double check inventory just in case someone else bought the last one
                   if (cartItem.Product!.Quantity<cartItem.Quantity)
                    throw new InvalidOperationException($"Product {cartItem.Product.Name} is out of stock");
                //deduct the purchased quantity from the product stock    
                cartItem.Product.Quantity-=cartItem.Quantity;
                //add it to the final receipt
                order.OrderItems.Add(new OrderItem
                {
                    ProductId = cartItem.ProductId,
                    Quantity = cartItem.Quantity,
                    UnitPrice = cartItem.UnitPrice
                });

               
            }
            //save the order and clear the cart
            context.Orders.Add(order);
            context.CartItems.RemoveRange(cart.Items); //this clears the cart
            await context.SaveChangesAsync();
            return MapToDto(order);
        }
        public async Task<List<OrderDtos>> GetUserOrdersAsync(int userId)
        {
            var orders = await context.Orders
                .Include(o=>o.OrderItems)
                .ThenInclude(i=> i.Product)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)//newest first
                .ToListAsync();
            return orders.Select(MapToDto).ToList();
        }
        //helper method to convert Order to OrderDtos
        private OrderDtos MapToDto(Order order)
        {
            return new OrderDtos
            {
                Id = order.Id,
                OrderDate = order.OrderDate,
                TotalAmount = order.TotalAmount,
                OrderStatus = order.OrderStatus,
                Items = order.OrderItems.Select(i => new OrderItemDtos
                {
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };
        }
    }
}
