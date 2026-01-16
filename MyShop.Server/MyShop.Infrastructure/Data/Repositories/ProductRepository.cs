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

        public IQueryable<Product> GetProductQueryable(string? keyword, int? categoryId)
        {
            var baseQuery = _dbSet.AsQueryable();

            if (categoryId.HasValue && categoryId != 0)
                baseQuery = baseQuery.Where(p => p.CategoryId == categoryId);

            if (string.IsNullOrEmpty(keyword))
                return baseQuery;

            // Loại bỏ ký tự đặc biệt nguy hiểm cho tsquery (như & | ! < >) để tránh syntax error
            // Chỉ giữ lại chữ cái, số và khoảng trắng
            var sanitizedKeyword = System.Text.RegularExpressions.Regex.Replace(keyword, @"[^\w\s]", " ").Trim();

            if (string.IsNullOrWhiteSpace(sanitizedKeyword)) return baseQuery;

            //Chuẩn hóa keyword để hỗ trợ tìm kiếm prefix
            //Ví dụ user gõ: "áo thu" -> convert thành: "áo:* & thu:*"
            //prefix matching
            var searchTerms = keyword.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var tsQuery = string.Join(" & ", searchTerms.Select(t => $"{t}:*"));

            // --- CHIẾN THUẬT 1: FTS + Prefix + Ranking ---
            var ftsQuery = baseQuery.Where(p => p.SearchVector.Matches(
                EF.Functions.ToTsQuery("vn_unaccent", tsQuery))
            );

            if (ftsQuery.Any())
            {
                return ftsQuery.OrderByDescending(p => p.SearchVector.Rank(
                    EF.Functions.ToTsQuery("vn_unaccent", tsQuery))
                );
            }

            // --- CHIẾN THUẬT 2: Fallback Trigram (Similarity) ---
            return baseQuery
                .Where(p => EF.Functions.TrigramsSimilarity(p.Name, keyword) > 0.3)
                .OrderByDescending(p => EF.Functions.TrigramsSimilarity(p.Name, keyword));
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
            string intervalStr = interval.ToString().ToLower();

            // 1. Logic EndDate: Đảm bảo lấy đến cuối ngày
            DateTime adjustedEnd = end;
            if (end.Hour == 0 && end.Minute == 0 && end.Second == 0)
            {
                if (interval == ReportTimeInterval.Year)
                    adjustedEnd = new DateTime(end.Year, 12, 31, 23, 59, 59);
                else if (interval == ReportTimeInterval.Month)
                {
                    var days = DateTime.DaysInMonth(end.Year, end.Month);
                    adjustedEnd = new DateTime(end.Year, end.Month, days, 23, 59, 59);
                }
                else
                    adjustedEnd = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            }

            // 2. [QUAN TRỌNG] Chuyển đổi tham số thành chuỗi cứng (ISO 8601)
            // Việc này ngăn chặn Npgsql can thiệp vào giờ giấc.
            string startStr = start.ToString("yyyy-MM-dd HH:mm:ss");
            string endStr = adjustedEnd.ToString("yyyy-MM-dd HH:mm:ss");

            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            // 3. [SỬA SQL] Ép kiểu triệt để
            // o.created_at::timestamp : Lột bỏ múi giờ của DB, chỉ giữ lại mặt số (VD: 10:00+07 -> 10:00)
            // TO_CHAR(...) : DB tự format nhãn, C# chỉ việc hiển thị (tránh lỗi lệch ngày khi read)

            string dateFormat = "DD/MM"; // Format mặc định cho Ngày/Tuần
            if (interval == ReportTimeInterval.Year) dateFormat = "YYYY";
            if (interval == ReportTimeInterval.Month) dateFormat = "MM/YYYY";

            command.CommandText = $@"
                SELECT 
                    -- Trả về ngày chuẩn (để sort)
                    date_trunc(@intervalStr, o.created_at::timestamp) as raw_date,
                    -- Trả về Nhãn dạng chuỗi (để hiển thị)
                    TO_CHAR(date_trunc(@intervalStr, o.created_at::timestamp), '{dateFormat}') as label_str,
                    SUM(oi.quantity) as total_quantity
                FROM ""order"" o
                JOIN order_item oi ON o.order_id = oi.order_id
        
                -- So sánh Timestamp thuần vs Timestamp thuần
                WHERE o.created_at::timestamp >= '{startStr}'::timestamp 
                  AND o.created_at::timestamp <= '{endStr}'::timestamp
          
                GROUP BY 1, 2
                ORDER BY 1";

            var pInterval = command.CreateParameter();
            pInterval.ParameterName = "@intervalStr";
            pInterval.Value = intervalStr;
            command.Parameters.Add(pInterval);

            // [DEBUG] Hãy copy dòng này in ra console server để xem SQL thực tế chạy là gì!
            Console.WriteLine($"DEBUG SQL: {command.CommandText}");

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                // Lưu ý: Cột 0 là raw_date (DateTime), Cột 1 là label_str (String)
                var rawDate = reader.GetDateTime(0);
                var label = reader.GetString(1); // Lấy trực tiếp label từ SQL

                result.Add(new ReportDataPoint
                {
                    Date = rawDate,
                    Label = label, // Dùng luôn label của SQL
                    Value = Convert.ToDouble(reader.GetDecimal(2)) // Cột 2 là total
                });
            }
            return result;
        }

        public async Task<List<ProductSeries>> GetProductComparisonReportAsync(DateTime start, DateTime end, ReportTimeInterval interval)
        {
            var seriesList = new Dictionary<string, ProductSeries>();
            var colors = new[] { "#6495ED", "#FFA500", "#008000", "#FF0000", "#800080", "#FF1493" }; // Thêm vài màu cho phong phú
            int colorIndex = 0;
            string intervalStr = interval.ToString().ToLower();

            // 1. Xử lý EndDate: Lấy đến 23:59:59
            DateTime adjustedEnd = end;
            if (end.Hour == 0 && end.Minute == 0 && end.Second == 0)
            {
                if (interval == ReportTimeInterval.Year)
                    adjustedEnd = new DateTime(end.Year, 12, 31, 23, 59, 59);
                else if (interval == ReportTimeInterval.Month)
                {
                    var days = DateTime.DaysInMonth(end.Year, end.Month);
                    adjustedEnd = new DateTime(end.Year, end.Month, days, 23, 59, 59);
                }
                else
                    adjustedEnd = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            }

            // 2. Biến ngày thành chuỗi cứng (Chặn đứng Npgsql can thiệp)
            string startStr = start.ToString("yyyy-MM-dd HH:mm:ss");
            string endStr = adjustedEnd.ToString("yyyy-MM-dd HH:mm:ss");

            // 3. Format nhãn cho SQL
            string dateFormat = "DD/MM";
            if (interval == ReportTimeInterval.Year) dateFormat = "YYYY";
            if (interval == ReportTimeInterval.Month) dateFormat = "MM/YYYY";

            var connection = _context.Database.GetDbConnection();
            using var command = connection.CreateCommand();

            // 4. SQL Query: Ép kiểu ::timestamp và dùng chuỗi
            command.CommandText = $@"
        SELECT 
            p.name,
            date_trunc(@intervalStr, o.created_at::timestamp) as raw_date,
            TO_CHAR(date_trunc(@intervalStr, o.created_at::timestamp), '{dateFormat}') as label_str,
            SUM(oi.quantity) as total_quantity
        FROM ""order"" o
        JOIN order_item oi ON o.order_id = oi.order_id
        JOIN product p ON oi.product_id = p.product_id
        
        WHERE o.created_at::timestamp >= '{startStr}'::timestamp 
          AND o.created_at::timestamp <= '{endStr}'::timestamp
          
        GROUP BY p.name, 2, 3
        ORDER BY p.name, 2";

            var pInterval = command.CreateParameter();
            pInterval.ParameterName = "@intervalStr";
            pInterval.Value = intervalStr;
            command.Parameters.Add(pInterval);

            if (connection.State != ConnectionState.Open) await connection.OpenAsync();
            using var reader = await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                string pName = reader.GetString(0);
                DateTime date = reader.GetDateTime(1);
                string label = reader.GetString(2); // Lấy nhãn từ SQL
                double value = Convert.ToDouble(reader.GetDecimal(3));

                if (!seriesList.ContainsKey(pName))
                {
                    seriesList[pName] = new ProductSeries
                    {
                        ProductName = pName,
                        ColorHex = colors[colorIndex++ % colors.Length],
                        Points = new List<ReportDataPoint>()
                    };
                }

                seriesList[pName].Points.Add(new ReportDataPoint
                {
                    Date = date,
                    Label = label,
                    Value = value
                });
            }
            return seriesList.Values.ToList();
        }
        public async Task<List<ReportDataPoint>> GetRevenueProfitReportAsync(DateTime start, DateTime end, ReportTimeInterval interval)
        {
            var result = new List<ReportDataPoint>();
            string intervalStr = interval.ToString().ToLower();

            // 1. Xử lý EndDate
            DateTime adjustedEnd = end;
            if (end.Hour == 0 && end.Minute == 0 && end.Second == 0)
            {
                if (interval == ReportTimeInterval.Year)
                    adjustedEnd = new DateTime(end.Year, 12, 31, 23, 59, 59);
                else if (interval == ReportTimeInterval.Month)
                {
                    var days = DateTime.DaysInMonth(end.Year, end.Month);
                    adjustedEnd = new DateTime(end.Year, end.Month, days, 23, 59, 59);
                }
                else
                    adjustedEnd = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            }

            // 2. String Injection
            string startStr = start.ToString("yyyy-MM-dd HH:mm:ss");
            string endStr = adjustedEnd.ToString("yyyy-MM-dd HH:mm:ss");

            // 3. Label Format
            string dateFormat = "DD/MM";
            if (interval == ReportTimeInterval.Year) dateFormat = "YYYY";
            if (interval == ReportTimeInterval.Month) dateFormat = "MM/YYYY";

            var connectionString = _context.Database.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            // 4. [SỬA TÊN CỘT TẠI ĐÂY]
            // Chuyển FinalPrice -> final_price
            // Chuyển TotalPrice -> total_price
            // Chuyển UnitCost -> unit_cost
            // Chuyển Quantity -> quantity

            command.CommandText = $@"
                SELECT 
                    date_trunc(@intervalStr, o.created_at::timestamp) as raw_date,
                    TO_CHAR(date_trunc(@intervalStr, o.created_at::timestamp), '{dateFormat}') as label_str,
            
                    -- Sửa tên cột cho đúng chuẩn Postgres (snake_case)
                    SUM(o.final_price) as revenue,
            
                    -- Tính lợi nhuận: Tổng tiền - (Số lượng * Giá vốn)
                    SUM(oi.total_price - (oi.quantity * oi.unit_cost)) as profit

                FROM ""order"" o
                JOIN order_item oi ON o.order_id = oi.order_id
        
                WHERE o.created_at::timestamp >= '{startStr}'::timestamp 
                  AND o.created_at::timestamp <= '{endStr}'::timestamp
                  -- AND o.status = 1 -- (Bỏ comment nếu cần lọc đơn đã thanh toán)
          
                GROUP BY 1, 2
                ORDER BY 1";

            var pInterval = command.CreateParameter();
            pInterval.ParameterName = "@intervalStr";
            pInterval.Value = intervalStr;
            command.Parameters.Add(pInterval);

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    result.Add(new ReportDataPoint
                    {
                        Date = reader.GetDateTime(0),
                        Label = reader.GetString(1),
                        Value = Convert.ToDouble(reader.GetDecimal(2)),
                        Profit = Convert.ToDouble(reader.GetDecimal(3))
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error: {command.CommandText}");
                // Mẹo: Hãy xem kỹ ex.Message, nó sẽ gợi ý cột đúng là gì (VD: Hint: Perhaps you meant to reference the column "o.FinalPrice")
                throw new Exception($"Lỗi cột DB: {ex.Message}");
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
                ColorHex = GetHexColorByIndex(colorIndex),
                Points = new List<ReportDataPoint>() // Khởi tạo list
            };

            string intervalStr = interval.ToString().ToLower();

            // 1. Xử lý EndDate: Lấy đến 23:59:59 của ngày kết thúc
            DateTime adjustedEnd = end;
            if (end.Hour == 0 && end.Minute == 0 && end.Second == 0)
            {
                if (interval == ReportTimeInterval.Year)
                    adjustedEnd = new DateTime(end.Year, 12, 31, 23, 59, 59);
                else if (interval == ReportTimeInterval.Month)
                {
                    var days = DateTime.DaysInMonth(end.Year, end.Month);
                    adjustedEnd = new DateTime(end.Year, end.Month, days, 23, 59, 59);
                }
                else
                    adjustedEnd = new DateTime(end.Year, end.Month, end.Day, 23, 59, 59);
            }

            // 2. Chuyển ngày thành chuỗi cứng (Để SQL xử lý, tránh Npgsql đổi múi giờ)
            string startStr = start.ToString("yyyy-MM-dd HH:mm:ss");
            string endStr = adjustedEnd.ToString("yyyy-MM-dd HH:mm:ss");

            // 3. Format nhãn hiển thị ngay trong SQL (cho đồng bộ)
            string dateFormat = "DD/MM";
            if (interval == ReportTimeInterval.Year) dateFormat = "YYYY";
            if (interval == ReportTimeInterval.Month) dateFormat = "MM/YYYY";

            // 4. Tạo kết nối riêng (Tránh lỗi DataReader conflict)
            var connectionString = _context.Database.GetConnectionString();
            using var connection = new Npgsql.NpgsqlConnection(connectionString);
            await connection.OpenAsync();

            using var command = connection.CreateCommand();

            // 5. Câu SQL: Dùng ::timestamp và nhúng chuỗi ngày vào
            command.CommandText = $@"
                SELECT 
                    date_trunc(@intervalStr, o.created_at::timestamp) as raw_date,
                    TO_CHAR(date_trunc(@intervalStr, o.created_at::timestamp), '{dateFormat}') as label_str,
                    SUM(oi.quantity) as total_quantity
                FROM ""order"" o
                JOIN order_item oi ON o.order_id = oi.order_id
                JOIN product p ON oi.product_id = p.product_id
            
                WHERE p.name = @pName 
                  AND o.created_at::timestamp >= '{startStr}'::timestamp 
                  AND o.created_at::timestamp <= '{endStr}'::timestamp
            
                GROUP BY 1, 2
                ORDER BY 1";

            // Tham số ProductName
            var pName = command.CreateParameter();
            pName.ParameterName = "@pName";
            pName.Value = productName;
            command.Parameters.Add(pName);

            // Tham số Interval
            var pInterval = command.CreateParameter();
            pInterval.ParameterName = "@intervalStr";
            pInterval.Value = intervalStr;
            command.Parameters.Add(pInterval);

            try
            {
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var date = reader.GetDateTime(0);
                    var label = reader.GetString(1); // Lấy Label chuẩn từ SQL
                    var val = Convert.ToDouble(reader.GetDecimal(2));

                    series.Points.Add(new ReportDataPoint
                    {
                        Date = date,
                        Label = label,
                        Value = val
                    });
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"SQL Error: {ex.Message}");
                throw;
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
