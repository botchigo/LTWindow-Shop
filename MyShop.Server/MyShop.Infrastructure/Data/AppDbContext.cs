using Microsoft.EntityFrameworkCore;
using MyShop.Domain.Entities;
using MyShop.Infrastructure.Data.Configurations;
using MyShop.Shared.Enums;

namespace MyShop.Infrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //global configure
            DefaultDecimalPrecisionConfiguration.Configure(modelBuilder);

            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("app_user");
                entity.HasKey(e => e.UserId);

                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd();

                entity.Property(e => e.Username)
                    .HasColumnName("username")
                    .HasMaxLength(50)
                    .IsRequired();

                entity.Property(e => e.Password)
                    .HasColumnName("password")
                    .HasMaxLength(100)
                    .IsRequired();

                entity.Property(e => e.FullName)
                    .HasColumnName("full_name")
                    .HasMaxLength(100);

                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp with time zone")
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAdd();

                entity.HasIndex(e => e.Username).IsUnique();
            });

            modelBuilder.Entity<AppUser>().HasData(
                new AppUser()
                {
                    UserId = 1,
                    Username = "admin",
                    Password = "123",
                    FullName = "admin",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                });

            //category
            modelBuilder.Entity<Category>().HasData(
                new Category
                {
                    Id = 1,
                    Name = "Laptop",
                    Description = "Laptop các loại",
                    CreatedAt = DateTime.UtcNow
                },
                new Category
                {
                    Id = 2,
                    Name = "Phụ kiện",
                    Description = "Phụ kiện máy tính",
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );

            //product
            modelBuilder.HasPostgresExtension("unaccent");

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasGeneratedTsVectorColumn(
                    p => p.SearchVector,
                    "vn_unaccent",
                    p => new { p.Name, p.Description })
                .HasIndex(p => p.SearchVector)
                .HasMethod("GIN");
            });

            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Name);

                entity.HasMany(p => p.ProductImages)
                    .WithOne(i => i.Product)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Sku = "LAP001",
                    Name = "MacBook Pro M3",
                    ImportPrice = 25000000,
                    SalePrice = 30000000,
                    Stock = 10,
                    SaleAmount = 0,
                    Description = "MacBook Pro chip M3",
                    CategoryId = 1,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                },
                new Product
                {
                    Id = 2,
                    Sku = "ACC001",
                    Name = "Chuột Logitech",
                    ImportPrice = 300000,
                    SalePrice = 450000,
                    Stock = 50,
                    SaleAmount = 30,
                    Description = "Chuột không dây Logitech",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
            // Seed 7 Orders rải đều các ngày trong tháng 12/2025
            var orders = new List<Order>();
            var orderItems = new List<OrderItem>();
            int userId = 1; // Giả định admin user có ID = 1 

            for (int i = 1; i <= 7; i++)
            {
                // Tạo ngày cách nhau 3 ngày để biểu đồ có độ dốc
                var date = new DateTime(2025, 12, i * 3, 10, 0, 0, DateTimeKind.Utc);
                decimal price = 500000 * i; // Giá tăng dần

                orders.Add(new Order
                {
                    Id = 100 + i,
                    UserId = userId,
                    FinalPrice = price,
                    Status = OrderStatus.Paid,
                    PaymentMethod = PaymentMethod.COD,
                    CreatedAt = date,
                    UpdatedAt = date
                });

                // Mỗi đơn hàng kèm theo 1 sản phẩm tương ứng
                orderItems.Add(new OrderItem
                {
                    Id = 100 + i,
                    OrderId = 100 + i,
                    ProductId = (i % 2 == 0) ? 1 : 2, // Luân phiên sản phẩm 1 và 2 
                    Quantity = i,
                    UnitSalePrice = price / i,
                    UnitCost = (price / i) * 0.7m, // Giả định giá vốn bằng 70% giá bán
                    TotalPrice = price
                });
            }

            modelBuilder.Entity<Order>().HasData(orders);
            modelBuilder.Entity<OrderItem>().HasData(orderItems);
        }

        //custom
        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HandleAuditing();
            return base.SaveChangesAsync(cancellationToken);
        }

        public override int SaveChanges()
        {
            HandleAuditing();
            return base.SaveChanges();
        }

        private void HandleAuditing()
        {
            var now = DateTime.UtcNow;

            var auditableEntries = ChangeTracker
                .Entries<AuditableEntity>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var auditableEntry in auditableEntries)
            {
                auditableEntry.Entity.UpdatedAt = now;
                if (auditableEntry.State == EntityState.Added)
                    auditableEntry.Entity.CreatedAt = now;
            }
        }
    }
}
