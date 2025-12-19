using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Database
{
  
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            
          
            var connectionString = "Host=localhost;Port=5432;Database=MyShop;Username=postgres;Password=12345";
            
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }

      
        public static AppDbContext CreateDbContext(string connectionString)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);
            return new AppDbContext(optionsBuilder.Options);
        }

        public static AppDbContext CreateDbContext(string host, int port, string database, string username, string password)
        {
            var connectionString = $"Host={host};Port={port};Database={database};Username={username};Password={password}";
            return CreateDbContext(connectionString);
        }
    }
}
