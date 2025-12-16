using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database
{
    /// <summary>
    /// Factory ð? t?o AppDbContext cho design-time (migrations, scaffolding)
    /// </summary>
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
            // Connection string m?c ð?nh cho development
            // Thay ð?i theo c?u h?nh c?a b?n
            var connectionString = "Host=localhost;Port=5432;Database=WindowApp_MyShop;Username=postgres;Password=23120138";
            
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// T?o DbContext v?i connection string tùy ch?nh
        /// </summary>
        public static AppDbContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }

        /// <summary>
        /// T?o DbContext v?i các tham s? riêng l?
        /// </summary>
        public static AppDbContext CreateDbContext(string host, int port, string database, string username, string password)
        {
            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            return CreateDbContext(connectionString);
        }
    }
}
