using Microsoft.EntityFrameworkCore;
using System.Text.Json;
// Nhớ using namespace chứa DbContext và Entities của bạn
using MyShop.Infrastructure.Data;
using MyShop.Domain.Entities;

namespace MyShop.Infrastructure.Data.Seed
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            // Đảm bảo database đã được tạo
            // await context.Database.EnsureCreatedAsync(); // Dùng dòng này nếu không dùng Migration
            // await context.Database.MigrateAsync(); // Dùng dòng này nếu dùng Migration (khuyên dùng)

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // Lấy đường dẫn gốc nơi file .exe/.dll chạy
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var seedFolder = Path.Combine(baseDir, "Data", "Seed");

            // 1. SEED CATEGORIES
            if (!context.Categories.Any())
            {
                var filePath = Path.Combine(seedFolder, "categories.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var categories = JsonSerializer.Deserialize<List<Category>>(json, options);

                    if (categories != null)
                    {
                        await context.Categories.AddRangeAsync(categories);
                        await context.SaveChangesAsync();

                        // Reset ID tự tăng cho PostgreSQL
                        await ResetPostgresSequence(context, "category", "category_id");
                    }
                }
            }

            // 2. SEED PRODUCTS (Chỉ chạy khi Category đã có data)
            if (!context.Products.Any())
            {
                var filePath = Path.Combine(seedFolder, "products.json");
                if (File.Exists(filePath))
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    var products = JsonSerializer.Deserialize<List<Product>>(json, options);

                    if (products != null)
                    {
                        await context.Products.AddRangeAsync(products);
                        await context.SaveChangesAsync();

                        // Reset ID tự tăng cho Product và Image
                        await ResetPostgresSequence(context, "product", "product_id");
                        await ResetPostgresSequence(context, "product_image", "image_id");
                    }
                }
            }
        }

        // Hàm reset bộ đếm ID trong PostgreSQL (BẮT BUỘC KHI HARD-CODE ID)
        private static async Task ResetPostgresSequence(DbContext context, string tableName, string idColumnName)
        {
            // Lệnh này set giá trị bộ đếm = MAX(id) hiện tại
            var sql = $"SELECT setval(pg_get_serial_sequence('{tableName}', '{idColumnName}'), COALESCE(MAX({idColumnName}), 1)) FROM {tableName};";
            await context.Database.ExecuteSqlRawAsync(sql);
        }
    }
}
