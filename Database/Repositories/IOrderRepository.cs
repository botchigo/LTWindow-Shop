using Database.Enums;
using Database.models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public interface IOrderRepository
    {
        Task<(List<Order> Orders, int TotalCount)> GetPagedOrdersAsync(
            int page, int pageSize,
            DateTime? fromDate, DateTime? toDate);

        Task<Order?> GetDetailsAsync(int orderId);
        Task UpdateStatusAsync(int orderId, OrderStatus newStatus);
        Task DeleteOrderAsync(int orderId);
        Task CreateOrderAsync(Order order);
    }
}
