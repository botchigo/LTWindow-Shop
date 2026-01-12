using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Orders;

namespace MyShop.Application.Interfaces
{
    public interface IOrderService
    {
        Task<Order> CreateOrderAsync(AddOrderDTO request);
        Task<Order> DeleteOrderAsync(int orderId);
        Task<Order> UpdateStatusAsync(UpdateOrderStatusDTO request);
    }
}
