using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.Enums;

namespace MyShop.Domain.Interfaces
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<Order?> GetDetailsAsync(int id);
        Task<int> GetTotalOrdersTodayAsync();
        Task<decimal> GetTotalRevenueTodayAsync();
        Task<List<RecentOrder>> GetLatest3OrdersAsync();
        Task<List<DailyRevenue>> GetRevenueByDayCurrentMonthAsync();
    }
}
