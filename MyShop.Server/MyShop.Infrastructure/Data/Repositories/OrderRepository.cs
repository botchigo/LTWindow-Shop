using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Infrastructure.Data;
using MyShop.Infrastructure.Data.Repositories;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.Enums;
using System.Data;

namespace Database.Repositories
{
    public class OrderRepository : GenericRepository<Order>, IOrderRepository
    {
        private readonly IMapper _mapper;
        public OrderRepository(AppDbContext context, ILogger<OrderRepository> logger, IMapper mapper) 
            : base(context, logger)
        {
            _mapper = mapper;
        }

        public async Task<Order?> GetDetailsAsync(int id)
        {
            return await _dbSet
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<int> GetTotalOrdersTodayAsync()
        {
            return await _dbSet
                .Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueTodayAsync()
        {
            return await _dbSet
                .Where(o => o.CreatedAt.Date == DateTime.UtcNow.Date && o.Status != OrderStatus.Canceled)
                .SumAsync(o => o.FinalPrice);
        }

        public async Task<List<RecentOrder>> GetLatest3OrdersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .Take(3)
                .ProjectTo<RecentOrder>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<DailyRevenue>> GetRevenueByDayCurrentMonthAsync()
        {
            var now = DateTime.UtcNow;
            var startOfMonth = new DateTime(now.Year, now.Month, 1);
            var endOfMonth = startOfMonth.AddMonths(1);

            return await _dbSet
                .AsNoTracking()
                .Where(o => o.CreatedAt >= startOfMonth && o.CreatedAt < endOfMonth && o.Status != OrderStatus.Canceled)
                .GroupBy(o => o.CreatedAt)
                .Select(g => new DailyRevenue
                {
                    Day = g.Key,
                    TotalRevenue = g.Sum(o => o.FinalPrice)
                })
                .OrderBy(o => o.Day)
                .ToListAsync();
        }
    }
}
