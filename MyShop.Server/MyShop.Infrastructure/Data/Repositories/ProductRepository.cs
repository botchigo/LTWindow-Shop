using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;
using MyShop.Shared.DTOs.Dashboards;
using MyShop.Shared.DTOs.Reports;
using MyShop.Shared.Enums;
using System.Data;

namespace MyShop.Infrastructure.Data.Repositories
{
    public class ProductRepository : GenericRepository<Product>, IProductRepository
    {
        private readonly IMapper _mapper;
        public ProductRepository(AppDbContext context, ILogger<ProductRepository> logger, IMapper mapper)
            : base(context, logger)
        {
            _mapper = mapper;
        }

        public async Task<List<Product>> GetByIdsAsync(List<int> ids)
        {
            return await _dbSet.Where(p => ids.Contains(p.Id)).ToListAsync();
        }

        public async Task<Product?> GetWithImagesAsync(int id)
        {
            return await _dbSet.Include(p => p.ProductImages).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> IsCategoryUsedAsync(int id)
        {
            return await _dbSet.AnyAsync(p => p.CategoryId == id);
        }

        public async Task<List<string>> GetAllSkuAsync()
        {
            return await _dbSet.Select(p => p.Sku).ToListAsync();
        }

        public async Task<bool> IsDuplicatedSku(string sku)
        {
            return await _dbSet.AnyAsync(p => p.Sku == sku);
        }

        public async Task<Product?> GetProductDetailsAsync(int productId)
        {
            return await _dbSet
                .Include(p => p.Category)
                .Include(p => p.ProductImages)
                .FirstOrDefaultAsync(p => p.Id == productId);
        }

        //report

        public async Task<List<ReportDataPoint>> GetProductSalesReportAsync(DateTime start, DateTime end, ReportTimeInterval interval)
        {
            var result = new List<ReportDataPoint>();
            string intervalStr = interval.ToString().ToLower(); // 'day', 'week', etc.

            // Sử dụng Raw SQL với date_trunc của PostgreSQL để gộp nhóm thời gian hiệu quả
            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT 
                    date_trunc('{intervalStr}', o.created_at) as period,
                    SUM(oi.quantity) as total_quantity
                FROM ""order"" o
                JOIN order_item oi ON o.order_id = oi.order_id
                WHERE o.created_at >= @start AND o.created_at <= @end
                GROUP BY period
                ORDER BY period";

            var pStart = command.CreateParameter(); pStart.ParameterName = "@start"; pStart.Value = start.ToUniversalTime();
            var pEnd = command.CreateParameter(); pEnd.ParameterName = "@end"; pEnd.Value = end.ToUniversalTime();
            command.Parameters.Add(pStart); command.Parameters.Add(pEnd);

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var date = reader.GetDateTime(0);
                result.Add(new ReportDataPoint
                {
                    Date = date,
                    Label = FormatLabel(date, interval),
                    Value = Convert.ToDouble(reader.GetDecimal(1))
                });
            }
            return result;
        }

        public async Task<List<ProductSeries>> GetProductComparisonReportAsync(DateTime start, DateTime end, ReportTimeInterval interval)
        {
            var seriesList = new Dictionary<string, ProductSeries>();
            var colors = new[]
{
                "#6495ED", // CornflowerBlue
                "#FFA500", // Orange
                "#008000", // Green
                "#FF0000"  // Red
            };
            int colorIndex = 0;

            string intervalStr = interval.ToString().ToLower();
            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                            SELECT 
                                p.name,
                                date_trunc('{intervalStr}', o.created_at) as period,
                                SUM(oi.quantity) as total_quantity
                            FROM ""order"" o
                            JOIN order_item oi ON o.order_id = oi.order_id
                            JOIN product p ON oi.product_id = p.product_id
                            WHERE o.created_at >= @start AND o.created_at <= @end
                            GROUP BY p.name, period
                            ORDER BY p.name, period";

            // ... (Thêm tham số @start, @end như trước)
            var pStart = command.CreateParameter(); pStart.ParameterName = "@start"; pStart.Value = start.ToUniversalTime();
            var pEnd = command.CreateParameter(); pEnd.ParameterName = "@end"; pEnd.Value = end.ToUniversalTime();
            command.Parameters.Add(pStart); command.Parameters.Add(pEnd);

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                string pName = reader.GetString(0);
                var date = reader.GetDateTime(1);
                if (!seriesList.ContainsKey(pName))
                {
                    seriesList[pName] = new ProductSeries
                    {
                        ProductName = pName,
                        ColorHex = colors[colorIndex++ % colors.Length]
                    };
                }
                seriesList[pName].Points.Add(new ReportDataPoint
                {
                    Date = reader.GetDateTime(1),
                    Label = FormatLabel(date, interval),
                    Value = Convert.ToDouble(reader.GetDecimal(2))
                });
            }
            return seriesList.Values.ToList();
        }
        
        public async Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(DateTime start, DateTime end, ReportTimeInterval interval)
        {
            var result = new List<ReportDataPoint>();
            string intervalStr = interval.ToString().ToLower();

            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();
            command.CommandText = $@"
                SELECT 
                    date_trunc('{intervalStr}', o.created_at) as period,
                    SUM(oi.total_price) as revenue,
                    SUM(oi.total_price - (oi.quantity * oi.unit_cost)) as profit
                FROM ""order"" o
                JOIN order_item oi ON o.order_id = oi.order_id
                WHERE o.created_at >= @start AND o.created_at <= @end
                GROUP BY period
                ORDER BY period";

            var pStart = command.CreateParameter(); pStart.ParameterName = "@start"; pStart.Value = start.ToUniversalTime();
            var pEnd = command.CreateParameter(); pEnd.ParameterName = "@end"; pEnd.Value = end.ToUniversalTime();
            command.Parameters.Add(pStart); command.Parameters.Add(pEnd);

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var date = reader.GetDateTime(0);
                result.Add(new ReportDataPoint
                {
                    Date = date,
                    Label = FormatLabel(date, interval),
                    Value = Convert.ToDouble(reader.GetDecimal(1)),
                    Profit = Convert.ToDouble(reader.GetDecimal(2))
                });
            }
            return result;
        }

        private string FormatLabel(DateTime date, ReportTimeInterval interval) => interval switch
        {
            ReportTimeInterval.Day => date.ToString("dd/MM"),
            ReportTimeInterval.Week => $"W{System.Globalization.ISOWeek.GetWeekOfYear(date)}",
            ReportTimeInterval.Month => date.ToString("MM/yyyy"),
            ReportTimeInterval.Year => date.ToString("yyyy"),
            _ => date.ToShortDateString()
        };

        public async Task<ProductSeries> GetSingleProductSeriesAsync(string productName, DateTime start, DateTime end, ReportTimeInterval interval, int colorIndex)
        {
            var series = new ProductSeries
            {
                ProductName = productName,
                // Danh sách màu để luân phiên
                ColorHex = GetHexColorByIndex(colorIndex)
            };

            string intervalStr = interval.ToString().ToLower();
            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            // Truy vấn lọc chính xác theo ProductName
            command.CommandText = $@"
                    SELECT 
                        date_trunc('{intervalStr}', o.created_at) as period,
                        SUM(oi.quantity) as total_quantity
                    FROM ""order"" o
                    JOIN order_item oi ON o.order_id = oi.order_id
                    JOIN product p ON oi.product_id = p.product_id
                    WHERE p.name = @pName AND o.created_at >= @start AND o.created_at <= @end
                    GROUP BY period
                    ORDER BY period";

            var pName = command.CreateParameter(); pName.ParameterName = "@pName"; pName.Value = productName;
            var pStart = command.CreateParameter(); pStart.ParameterName = "@start"; pStart.Value = start.ToUniversalTime();
            var pEnd = command.CreateParameter(); pEnd.ParameterName = "@end"; pEnd.Value = end.ToUniversalTime();

            command.Parameters.Add(pName);
            command.Parameters.Add(pStart);
            command.Parameters.Add(pEnd);

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var date = reader.GetDateTime(0);
                series.Points.Add(new ReportDataPoint
                {
                    Date = date,
                    // FormatLabel giúp hiển thị ngày đúng định dạng trên trục X
                    Label = FormatLabel(date, interval),
                    Value = Convert.ToDouble(reader.GetDecimal(1))
                });
            }
            return series;
        }

        private string GetHexColorByIndex(int index)
        {
            var colors = new[]
            {
                "#6495ED", // CornflowerBlue
                "#FFA500", // Orange
                "#008000", // Green
                "#FF0000", // Red
                "#800080", // Purple
                "#FF1493"  // DeepPink
            };

            return colors[index % colors.Length];
        }

        //dashboard
        public async Task<int> GetTotalProductsAsync()
        {
            return await _dbSet.CountAsync();
        }

        public async Task<List<LowStockProduct>> GetTop5LowStockAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderBy(p => p.Stock)
                    .ThenBy(p => p.Name)
                .Take(5)
                .ProjectTo<LowStockProduct>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }

        public async Task<List<BestSellerProduct>> GetTop5BestSellersAsync()
        {
            return await _dbSet
                .AsNoTracking()
                .OrderByDescending(p => p.SaleAmount)
                    .ThenBy(p => p.Name)
                .Take(5)
                .ProjectTo<BestSellerProduct>(_mapper.ConfigurationProvider)
                .ToListAsync();
        }              
    }
}
