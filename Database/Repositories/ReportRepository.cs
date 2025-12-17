using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Database.Enums;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    public class ReportDataPoint
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; } // Số lượng hoặc Doanh thu
        public double Profit { get; set; } // Chỉ dùng cho báo cáo Doanh thu/Lợi nhuận
        public DateTime Date { get; set; }
    }

    public interface IReportRepository
    {
        Task<List<ReportDataPoint>> GetProductSalesReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
        Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(DateTime start, DateTime end, ReportTimeInterval interval);
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
    }
}
