using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Enums;
using Microsoft.EntityFrameworkCore;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Database.Repositories
{
    public class ReportDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; } // Doanh thu
        public double Profit { get; set; } // Chỉ dùng cho báo cáo Doanh thu/Lợi nhuận
        public DateTime Date { get; set; }
    }

    public class ProductSeries
    {
        public string ProductName { get; set; } = string.Empty;
        public List<ReportDataPoint> Points { get; set; } = new();
        public Windows.UI.Color Color { get; set; } // Màu riêng cho mỗi đường
    }

    public interface IReportRepository
    {
        Task<List<ReportDataPoint>> GetProductSalesReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<List<ProductSeries>> GetProductComparisonReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<ProductSeries> GetSingleProductSeriesAsync(string productName, DateTime start, DateTime end, ReportTimeInterval interval, int colorIndex);
        Task<List<string>> GetAllProductNamesAsync(); // Để load vào ComboBox
    }

    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

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
            var colors = new[] { Microsoft.UI.Colors.CornflowerBlue, Microsoft.UI.Colors.Orange, Microsoft.UI.Colors.Green, Microsoft.UI.Colors.Red };
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
                        Color = colors[colorIndex++ % colors.Length]
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
                Color = GetColorByIndex(colorIndex)
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

        public async Task<List<string>> GetAllProductNamesAsync()
        {
            return await _context.Products.Select(p => p.Name).OrderBy(n => n).ToListAsync();
        }
        // Hàm bổ trợ lấy màu theo chỉ số để phân biệt các đường sản phẩm
        private Windows.UI.Color GetColorByIndex(int index)
        {
            var colors = new[] {
        Microsoft.UI.Colors.CornflowerBlue,
        Microsoft.UI.Colors.Orange,
        Microsoft.UI.Colors.Green,
        Microsoft.UI.Colors.Red,
        Microsoft.UI.Colors.Purple,
        Microsoft.UI.Colors.DeepPink
    };
            return colors[index % colors.Length];
        }
    }
}
