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

            //product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasIndex(p => p.Name);

                entity.HasMany(p => p.ProductImages)
                    .WithOne(i => i.Product)
                    .HasForeignKey(i => i.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
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
