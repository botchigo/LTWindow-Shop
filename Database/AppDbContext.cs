using Database.models;
using Microsoft.EntityFrameworkCore;

namespace Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // C?u h?nh cho AppUser
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
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAdd();
                
                entity.HasIndex(e => e.Username).IsUnique();
            });

            // C?u h?nh cho Category
            modelBuilder.Entity<Category>(entity =>
            {
                entity.ToTable("category");
                entity.HasKey(e => e.CategoryId);
                
                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(100)
                    .IsRequired();
                
                entity.Property(e => e.Description)
                    .HasColumnName("description");
            });

            // C?u h?nh cho Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.ToTable("product");
                entity.HasKey(e => e.ProductId);
                
                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.CategoryId)
                    .HasColumnName("category_id");
                
                entity.Property(e => e.Sku)
                    .HasColumnName("sku")
                    .HasMaxLength(50)
                    .IsRequired();
                
                entity.Property(e => e.Name)
                    .HasColumnName("name")
                    .HasMaxLength(255)
                    .IsRequired();
                
                entity.Property(e => e.ImportPrice)
                    .HasColumnName("import_price")
                    .IsRequired();
                
                entity.Property(e => e.SalePrice)
                    .HasColumnName("sale_price")
                    .IsRequired();
                
                entity.Property(e => e.Count)
                    .HasColumnName("count")
                    .HasDefaultValue(0);
                
                entity.Property(e => e.Description)
                    .HasColumnName("description");
                
                entity.Property(e => e.CreatedAt)
                    .HasColumnName("created_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("NOW()")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.UpdatedAt)
                    .HasColumnName("updated_at")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("NOW()");
                
                entity.HasIndex(e => e.Sku).IsUnique();
                entity.HasIndex(e => e.Name);
                
                entity.HasOne(e => e.Category)
                    .WithMany(c => c.Products)
                    .HasForeignKey(e => e.CategoryId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // C?u h?nh cho Order (b?ng orders)
            modelBuilder.Entity<Order>(entity =>
            {
                entity.ToTable("orders");
                entity.HasKey(e => e.OrderId);
                
                entity.Property(e => e.OrderId)
                    .HasColumnName("order_id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.UserId)
                    .HasColumnName("user_id");
                
                entity.Property(e => e.CreatedTime)
                    .HasColumnName("created_time")
                    .HasColumnType("timestamp")
                    .HasDefaultValueSql("NOW()")
                    .IsRequired();
                
                entity.Property(e => e.FinalPrice)
                    .HasColumnName("final_price")
                    .HasDefaultValue(0)
                    .IsRequired();
                
                entity.Property(e => e.Status)
                    .HasColumnName("status")
                    .HasMaxLength(30)
                    .HasDefaultValue("paid");
                
                entity.Property(e => e.PaymentMethod)
                    .HasColumnName("payment_method")
                    .HasMaxLength(30)
                    .HasDefaultValue("cash");
                
                entity.HasIndex(e => e.CreatedTime);
                
                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // C?u h?nh cho OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.ToTable("order_item");
                entity.HasKey(e => e.OrderItemId);
                
                entity.Property(e => e.OrderItemId)
                    .HasColumnName("order_item_id")
                    .ValueGeneratedOnAdd();
                
                entity.Property(e => e.OrderId)
                    .HasColumnName("order_id");
                
                entity.Property(e => e.ProductId)
                    .HasColumnName("product_id");
                
                entity.Property(e => e.Quantity)
                    .HasColumnName("quantity")
                    .HasDefaultValue(1)
                    .IsRequired();
                
                entity.Property(e => e.UnitSalePrice)
                    .HasColumnName("unit_sale_price")
                    .IsRequired();
                
                entity.Property(e => e.UnitCost)
                    .HasColumnName("unit_cost")
                    .IsRequired();
                
                entity.Property(e => e.TotalPrice)
                    .HasColumnName("total_price")
                    .IsRequired();
                
                entity.HasIndex(e => e.ProductId);
                
                entity.HasOne(e => e.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(e => e.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);
                
                entity.HasOne(e => e.Product)
                    .WithMany(p => p.OrderItems)
                    .HasForeignKey(e => e.ProductId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
