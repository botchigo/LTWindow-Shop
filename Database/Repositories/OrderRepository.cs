using Database.Enums;
using Database.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Database.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly IDbContextFactory<AppDbContext> _dbContextFactory;

        public OrderRepository(IDbContextFactory<AppDbContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public async Task<(List<Order> Orders, int TotalCount)> GetPagedOrdersAsync(
            int page, int pageSize,
            DateTime? fromDate, DateTime? toDate)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var query = context.Orders.AsQueryable();

            //filter
            if(fromDate.HasValue)
            {
                var utcFrom = DateTime.SpecifyKind(fromDate.Value, DateTimeKind.Utc);
                query = query.Where(o => o.CreatedAt >= utcFrom);
            }
            if(toDate.HasValue)
            {
                var utcTo = DateTime.SpecifyKind(toDate.Value, DateTimeKind.Utc);
                var endOfDay = utcTo.Date.AddDays(1).AddTicks(-1);
                endOfDay = DateTime.SpecifyKind(endOfDay, DateTimeKind.Utc);

                query = query.Where(o => o.CreatedAt <= endOfDay);
            }

            int total = await query.CountAsync();

            //paging
            var orders = await query
                .OrderByDescending(o => o.CreatedAt)
                .Skip((page-1)* pageSize)
                .Take(pageSize)
                .AsNoTracking()
                .ToListAsync();

            return (orders, total);
        }

        public async Task<Order?> GetDetailsAsync(int orderId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            return await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                        .ThenInclude(p => p.ProductImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task CreateOrderAsync(Order order)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Orders.AddAsync(order);
                await context.SaveChangesAsync();

                foreach(var item in order.OrderItems)
                {
                    var product = await context.Products.FindAsync(item.ProductId);

                    if(product is null)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Sản phẩm ID {item.ProductId} không tồn tại.");
                    }

                    if(product.Stock < item.Quantity)
                    {
                        await transaction.RollbackAsync();
                        throw new Exception($"Sản phẩm '{product.Name}' không đủ hàng (Tồn: {product.Stock}).");
                    }

                    product.Stock -= item.Quantity;
                    product.SaleAmount++;
                    item.OrderId = order.Id;
                }

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateStatusAsync(int orderId, OrderStatus newStatus)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            var order = await context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order is null)
                throw new Exception("Không tìm thấy đơn hàng");

            if (!CanUpdateStatus(order.Status, newStatus))
                throw new Exception($"Không thể cập nhật từ trạng thái {order.Status} sang {newStatus}");

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {              
                //if you update a cancelled order, you need to get stock
                if(order.Status == OrderStatus.Canceled)
                {
                    foreach(var item in order.OrderItems)
                    {

                        if(item.Quantity > item.Product.Stock)
                        {
                            await transaction.RollbackAsync();
                            throw new Exception
                                ($"Không thể cập nhật từ trạng thái {order.Status} sang {newStatus} vì sản phảm này đã hết");
                        }

                        item.Product.Stock -= item.Quantity;
                        item.Product.SaleAmount++;
                    }
                }
                //if you update a order to a cancelled one, you need to restock
                else if (newStatus == OrderStatus.Canceled)
                {
                    foreach(var item in order.OrderItems)
                    {
                        item.Product.Stock += item.Quantity;
                        item.Product.SaleAmount--;
                    }
                }

                order.Status = newStatus;

                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception($"Cập nhật đơn hàng không thành công. Hãy thử lại.");
            }
        }

        public async Task DeleteOrderAsync(int orderId)
        {
            using var context = await _dbContextFactory.CreateDbContextAsync();

            using var transaction = await context.Database.BeginTransactionAsync();
            try
            {
                var order = await context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order is null)
                    throw new Exception("Không tìm thấy đơn hàng");

                if(order.Status != OrderStatus.Canceled)
                {
                    foreach(var item in order.OrderItems)
                    {
                        var product = await context.Products.FindAsync(item.ProductId);
                        if(product is not null)
                        {
                            product.Stock += item.Quantity;
                        }
                    }
                }

                context.Orders.Remove(order);
                await context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception($"Xóa đơn hàng không thành công. Hãy thử lại.");
            }
        }

        private bool CanUpdateStatus(OrderStatus oldStatus,  OrderStatus newStatus)
        {
            if(oldStatus == newStatus) return false;

            if(oldStatus == OrderStatus.Paid && newStatus != OrderStatus.Canceled) 
                return false;

            return true;
        }
    }
}
