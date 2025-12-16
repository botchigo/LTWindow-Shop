using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.models;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services
{
    /// <summary>
    /// Utility class ð? test và debug database connection
    /// </summary>
    public static class DatabaseDebugger
    {
        /// <summary>
        /// Test k?t n?i database v?i thông tin chi ti?t
        /// </summary>
        public static async Task<(bool success, string message)> TestDatabaseConnectionAsync()
        {
            try
            {
                var connectionString = "Host=localhost;Port=5432;Database=MyShop;Username=postgres;Password=12345";
                
                // Test 1: T?o context
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                
                using var context = new AppDbContext(optionsBuilder.Options);
                
                // Test 2: Ki?m tra k?t n?i
                bool canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return (false, "? Không th? k?t n?i t?i PostgreSQL server.\nKi?m tra:\n- PostgreSQL ðang ch?y?\n- Port 5432 có m? không?\n- Username/Password ðúng chýa?");
                }
                
                // Test 3: Ki?m tra database t?n t?i
                var databaseName = context.Database.GetDbConnection().Database;
                
                // Test 4: Ki?m tra b?ng app_user
                bool tableExists = await context.AppUsers.AnyAsync();
                
                return (true, $"? K?t n?i thành công!\n?? Database: {databaseName}\n?? Có {await context.AppUsers.CountAsync()} users trong database");
            }
            catch (Npgsql.NpgsqlException ex)
            {
                if (ex.Message.Contains("does not exist"))
                {
                    return (false, $"? Database 'MyShop' chýa ðý?c t?o.\n\nCh?y SQL sau trong pgAdmin:\nCREATE DATABASE \"MyShop\";\n\nChi ti?t l?i: {ex.Message}");
                }
                else if (ex.Message.Contains("password authentication failed"))
                {
                    return (false, $"? Sai m?t kh?u PostgreSQL.\n\nKi?m tra l?i password trong:\n- DatabaseManager.cs\n- AppDbContextFactory.cs\n\nChi ti?t: {ex.Message}");
                }
                else if (ex.Message.Contains("connection refused"))
                {
                    return (false, $"? PostgreSQL server không ch?y ho?c không th? k?t n?i.\n\nKi?m tra:\n- M? Services ? t?m 'postgresql' ? Start\n- Ho?c ch?y: net start postgresql-x64-18\n\nChi ti?t: {ex.Message}");
                }
                return (false, $"? L?i PostgreSQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"? L?i không xác ð?nh:\n{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}");
            }
        }

        /// <summary>
        /// T?o database và b?ng n?u chýa có
        /// </summary>
        public static async Task<(bool success, string message)> EnsureDatabaseCreatedAsync()
        {
            try
            {
                var connectionString = "Host=localhost;Port=5432;Database=MyShop;Username=postgres;Password=12345";
                
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                
                using var context = new AppDbContext(optionsBuilder.Options);
                
                // T?o database và b?ng n?u chýa có
                bool created = await context.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    return (true, "? Ð? t?o database và b?ng thành công!");
                }
                else
                {
                    return (true, "?? Database và b?ng ð? t?n t?i.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"? Không th? t?o database:\n{ex.Message}");
            }
        }

        /// <summary>
        /// Thêm d? li?u test
        /// </summary>
        public static async Task<(bool success, string message)> SeedTestDataAsync()
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var repo = dbManager.UserRepository;
                
                // Thêm admin n?u chýa có
                if (!await repo.IsUsernameExistsAsync("admin"))
                {
                    await repo.RegisterUserAsync("admin", "123456", "Administrator");
                }
                
                // Thêm test user n?u chýa có
                if (!await repo.IsUsernameExistsAsync("test"))
                {
                    await repo.RegisterUserAsync("test", "test123", "Test User");
                }
                
                return (true, "? Ð? thêm d? li?u test:\n- Username: admin / Password: 123456\n- Username: test / Password: test123");
            }
            catch (Exception ex)
            {
                return (false, $"? Không th? thêm d? li?u test: {ex.Message}");
            }
        }

        /// <summary>
        /// Hi?n th? t?t c? users trong database
        /// </summary>
        public static async Task<(bool success, string message)> ListAllUsersAsync()
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var context = dbManager.Context;
                
                var users = await context.AppUsers.ToListAsync();
                
                if (users.Count == 0)
                {
                    return (true, "?? Chýa có user nào trong database.");
                }
                
                var result = $"?? Danh sách {users.Count} users:\n\n";
                foreach (var user in users)
                {
                    result += $"• ID: {user.UserId} | Username: {user.Username} | Name: {user.FullName}\n";
                }
                
                return (true, result);
            }
            catch (Exception ex)
            {
                return (false, $"? L?i khi l?y danh sách users: {ex.Message}");
            }
        }

        /// <summary>
        /// Debug chi ti?t: Ki?m tra username và password trong database
        /// </summary>
        public static async Task<(bool success, string message)> DebugLoginAsync(string username, string password)
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var context = dbManager.Context;
                
                // Ki?m tra t?ng s? users
                var totalUsers = await context.AppUsers.CountAsync();
                
                // T?m user theo username
                var userByUsername = await context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                if (userByUsername == null)
                {
                    return (false, $"? Username '{username}' KHÔNG T?N T?I trong database!\n\n" +
                                   $"?? T?ng s? users: {totalUsers}\n\n" +
                                   $"?? Các username có s?n:\n" +
                                   $"{await GetAllUsernamesAsync(context)}");
                }
                
                // Ki?m tra password
                var storedPassword = userByUsername.Password;
                var passwordMatch = storedPassword == password;
                
                var result = $"?? DEBUG LOGIN:\n\n" +
                            $"Username nh?p: '{username}'\n" +
                            $"Password nh?p: '{password}'\n\n" +
                            $"Username trong DB: '{userByUsername.Username}'\n" +
                            $"Password trong DB: '{storedPassword}'\n\n" +
                            $"? Username kh?p: YES\n" +
                            $"{(passwordMatch ? "?" : "?")} Password kh?p: {(passwordMatch ? "YES" : "NO")}\n\n";
                
                if (!passwordMatch)
                {
                    result += $"? M?T KH?U KHÔNG KH?P!\n" +
                             $"B?n nh?p: '{password}' (length: {password.Length})\n" +
                             $"Trong DB: '{storedPassword}' (length: {storedPassword.Length})\n\n" +
                             $"Có th? do:\n" +
                             $"• Sai m?t kh?u\n" +
                             $"• Có kho?ng tr?ng th?a\n" +
                             $"• Caps Lock b?t";
                }
                else
                {
                    result += "? C? username và password ð?u ÐÚNG!";
                }
                
                return (passwordMatch, result);
            }
            catch (Exception ex)
            {
                return (false, $"? L?i debug: {ex.Message}");
            }
        }
        
        private static async Task<string> GetAllUsernamesAsync(AppDbContext context)
        {
            var users = await context.AppUsers
                .Select(u => new { u.Username, u.Password, u.FullName })
                .ToListAsync();
            
            if (users.Count == 0)
                return "  (không có user nào)";
            
            var result = "";
            foreach (var user in users)
            {
                result += $"  • '{user.Username}' / '{user.Password}' ({user.FullName})\n";
            }
            return result;
        }
    }
}
