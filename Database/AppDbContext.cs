using Database.Configurations;
using Database.models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Database
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //global configure
            DefaultDecimalPrecisionConfiguration.Configure(modelBuilder);

            // C?u h?nh cho AppUser
            modelBuilder.Entity<AppUser>(entity =>
            {
                entity.ToTable("app_user");
                entity.HasKey(e => e.UserId);
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id")
                    .ValueGeneratedOnAdd(); // T? đ?ng tăng khi insert
                
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
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAdd(); // Dùng default value t? database
                
                entity.HasIndex(e => e.Username).IsUnique();
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
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Name);
            });
            modelBuilder.Entity<Product>().HasData(
                new Product
                {
                    Id = 1,
                    Sku = "LAP001",
                    Name = "MacBook Pro M3",
                    ImportPrice = 25000000,
                    SalePrice = 30000000,
                    Count = 10,
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
                    Count = 50,
                    Description = "Chuột không dây Logitech",
                    CategoryId = 2,
                    CreatedAt = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                }
            );
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

            foreach(var auditableEntry in auditableEntries )
            {
                auditableEntry.Entity.UpdatedAt = now;
                if(auditableEntry.State == EntityState.Added)
                    auditableEntry.Entity.CreatedAt = now;
            }
        }
    }
}
