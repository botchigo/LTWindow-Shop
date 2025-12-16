using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    /// <summary>
    /// Repository g?i các PostgreSQL functions cho Dashboard
    /// </summary>
    public class DashboardRepository
    {
        private readonly AppDbContext _context;

        public DashboardRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// L?y t?ng s? s?n ph?m - g?i fn_total_products()
        /// </summary>
        public async Task<int> GetTotalProductsAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT fn_total_products()";
                var result = await command.ExecuteScalarAsync();

                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTotalProductsAsync] Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// L?y top 5 s?n ph?m s?p h?t hàng - g?i fn_top5_low_stock()
        /// Fallback: Query tr?c ti?p n?u function không ho?t ð?ng
        /// Returns: product_id, name, stock
        /// </summary>
        public async Task<List<LowStockProduct>> GetTop5LowStockAsync()
        {
            var result = new List<LowStockProduct>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                
                // Th? g?i function trý?c
                command.CommandText = "SELECT * FROM fn_top5_low_stock()";
                
                try
                {
                    using var reader = await command.ExecuteReaderAsync();
                    
                    while (await reader.ReadAsync())
                    {
                        var productId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                        var name = reader.IsDBNull(1) ? "" : reader.GetString(1);
                        var stock = reader.IsDBNull(2) ? 0 : reader.GetInt32(2);
                        
                        System.Diagnostics.Debug.WriteLine($"[LowStock-Function] ProductId={productId}, Name='{name}', Stock={stock}");
                        
                        result.Add(new LowStockProduct
                        {
                            ProductId = productId,
                            Name = name,
                            Stock = stock
                        });
                    }
                }
                catch (Exception funcEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[GetTop5LowStockAsync] Function failed: {funcEx.Message}");
                    System.Diagnostics.Debug.WriteLine("[GetTop5LowStockAsync] Using direct query fallback...");
                    
                    // Fallback: Query tr?c ti?p t? b?ng product
                    using var fallbackCommand = connection.CreateCommand();
                    fallbackCommand.CommandText = @"
                        SELECT product_id, name, count as stock 
                        FROM product 
                        WHERE count <= 10 
                        ORDER BY count ASC, name ASC 
                        LIMIT 5";
                    
                    using var fallbackReader = await fallbackCommand.ExecuteReaderAsync();
                    while (await fallbackReader.ReadAsync())
                    {
                        var productId = fallbackReader.IsDBNull(0) ? 0 : fallbackReader.GetInt32(0);
                        var name = fallbackReader.IsDBNull(1) ? "" : fallbackReader.GetString(1);
                        var stock = fallbackReader.IsDBNull(2) ? 0 : fallbackReader.GetInt32(2);
                        
                        System.Diagnostics.Debug.WriteLine($"[LowStock-Fallback] ProductId={productId}, Name='{name}', Stock={stock}");
                        
                        result.Add(new LowStockProduct
                        {
                            ProductId = productId,
                            Name = name,
                            Stock = stock
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTop5LowStockAsync] Error: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// L?y top 5 s?n ph?m bán ch?y - g?i fn_top5_best_seller()
        /// Returns: product_id, product_name, total_quantity
        /// </summary>
        public async Task<List<BestSellerProduct>> GetTop5BestSellersAsync()
        {
            var result = new List<BestSellerProduct>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM fn_top5_best_seller()";
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new BestSellerProduct
                    {
                        ProductId = reader.GetInt32(reader.GetOrdinal("product_id")),
                        ProductName = reader.GetString(reader.GetOrdinal("product_name")),
                        TotalQuantity = reader.GetInt64(reader.GetOrdinal("total_quantity"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTop5BestSellersAsync] Error: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// L?y t?ng s? ðõn hàng trong ngày - g?i fn_total_orders_today()
        /// </summary>
        public async Task<int> GetTotalOrdersTodayAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT fn_total_orders_today()";
                var result = await command.ExecuteScalarAsync();

                return result != null ? Convert.ToInt32(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTotalOrdersTodayAsync] Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// L?y t?ng doanh thu trong ngày - g?i fn_total_revenue_today()
        /// Returns: BIGINT
        /// </summary>
        public async Task<long> GetTotalRevenueTodayAsync()
        {
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT fn_total_revenue_today()";
                var result = await command.ExecuteScalarAsync();

                return result != null && result != DBNull.Value ? Convert.ToInt64(result) : 0;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetTotalRevenueTodayAsync] Error: {ex.Message}");
                return 0;
            }
        }

        /// <summary>
        /// L?y 3 ðõn hàng g?n nh?t - g?i fn_latest_3_orders()
        /// Returns: order_id, created_time, final_price, status, payment_method
        /// </summary>
        public async Task<List<RecentOrder>> GetLatest3OrdersAsync()
        {
            var result = new List<RecentOrder>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM fn_latest_3_orders()";
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new RecentOrder
                    {
                        OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                        CreatedTime = reader.GetDateTime(reader.GetOrdinal("created_time")),
                        FinalPrice = reader.GetInt32(reader.GetOrdinal("final_price")),
                        Status = reader.GetString(reader.GetOrdinal("status")),
                        PaymentMethod = reader.GetString(reader.GetOrdinal("payment_method"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetLatest3OrdersAsync] Error: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// L?y doanh thu theo ngày trong tháng - g?i fn_revenue_by_day_current_month()
        /// Returns: day, total_revenue
        /// </summary>
        public async Task<List<DailyRevenue>> GetRevenueByDayCurrentMonthAsync()
        {
            var result = new List<DailyRevenue>();
            try
            {
                var connection = _context.Database.GetDbConnection();
                if (connection.State != ConnectionState.Open)
                    await connection.OpenAsync();

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT * FROM fn_revenue_by_day_current_month()";
                
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new DailyRevenue
                    {
                        Day = reader.GetDateTime(reader.GetOrdinal("day")),
                        TotalRevenue = reader.GetInt64(reader.GetOrdinal("total_revenue"))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[GetRevenueByDayCurrentMonthAsync] Error: {ex.Message}");
            }
            return result;
        }

        /// <summary>
        /// L?y t?t c? d? li?u dashboard
        /// </summary>
        public async Task<DashboardData> GetDashboardDataAsync()
        {
            return new DashboardData
            {
                TotalProducts = await GetTotalProductsAsync(),
                TodayOrders = await GetTotalOrdersTodayAsync(),
                TodayRevenue = await GetTotalRevenueTodayAsync(),
                LowStockProducts = await GetTop5LowStockAsync(),
                BestSellerProducts = await GetTop5BestSellersAsync(),
                RecentOrders = await GetLatest3OrdersAsync(),
                MonthlyRevenue = await GetRevenueByDayCurrentMonthAsync()
            };
        }
    }

    #region DTOs

    public class LowStockProduct
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Stock { get; set; }
    }

    public class BestSellerProduct
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public long TotalQuantity { get; set; }
    }

    public class RecentOrder
    {
        public int OrderId { get; set; }
        public DateTime CreatedTime { get; set; }
        public int FinalPrice { get; set; }
        public string Status { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class DailyRevenue
    {
        public DateTime Day { get; set; }
        public long TotalRevenue { get; set; }
    }

    public class DashboardData
    {
        public int TotalProducts { get; set; }
        public int TodayOrders { get; set; }
        public long TodayRevenue { get; set; }
        public List<LowStockProduct> LowStockProducts { get; set; } = new();
        public List<BestSellerProduct> BestSellerProducts { get; set; } = new();
        public List<RecentOrder> RecentOrders { get; set; } = new();
        public List<DailyRevenue> MonthlyRevenue { get; set; } = new();
    }

    #endregion
}
