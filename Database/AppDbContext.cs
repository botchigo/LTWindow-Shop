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
                    .ValueGeneratedOnAdd(); // T? ð?ng tãng khi insert
                
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
        }
    }
}
