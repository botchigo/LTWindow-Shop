using System;
using System.Linq;
using System.Threading.Tasks;
using Database;
using Database.models;
using Database.Repositories;
using Microsoft.EntityFrameworkCore;

namespace MyShop.Services
{
  
    public static class DatabaseDebugger
    {
        public static async Task<(bool success, string message)> TestDatabaseConnectionAsync()
        {
            try
            {
                var connectionString = "Host=localhost;Port=5432;Database=MyShop;Username=postgres;Password=12345";
                
                // Test 1: Tạo context
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                
                using var context = new AppDbContext(optionsBuilder.Options);
                
                // Test 2: Kiểm tra kết nối
                bool canConnect = await context.Database.CanConnectAsync();
                if (!canConnect)
                {
                    return (false, "Không thế kết nối vui lòng kiểm tra lại");
                }
                
                // Test 3: Ki?m tra database t?n t?i
                var databaseName = context.Database.GetDbConnection().Database;
                
                // Test 4: Ki?m tra b?ng app_user
                bool tableExists = await context.AppUsers.AnyAsync();
                
                return (true, $"Kết nối thành công!\n?? Database: {databaseName}\n?? Có {await context.AppUsers.CountAsync()} users trong database");
            }
            catch (Npgsql.NpgsqlException ex)
            {
                if (ex.Message.Contains("does not exist"))
                {
                    return (false, $"? Database 'MyShop' chưa được tạo.\n\nChạy SQL sau trong pgAdmin:\nCREATE DATABASE \"MyShop\";\n\nChi tiết lại: {ex.Message}");
                }
                else if (ex.Message.Contains("password authentication failed"))
                {
                    return (false, $"? Sai mật khẩu PostgreSQL.: {ex.Message}");
                }
                else if (ex.Message.Contains("connection refused"))
                {
                    return (false, $"? PostgreSQL server không chạy hoặc không thể kết nối: {ex.Message}");
                }
                return (false, $"? Lỗii PostgreSQL: {ex.Message}");
            }
            catch (Exception ex)
            {
                return (false, $"? Lỗi không xác đ?nh:\n{ex.Message}\n\nInner Exception: {ex.InnerException?.Message}");
            }
        }

      
        public static async Task<(bool success, string message)> EnsureDatabaseCreatedAsync()
        {
            try
            {
                var connectionString = "Host=localhost;Port=5432;Database=MyShop;Username=postgres;Password=12345";
                
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);
                
                using var context = new AppDbContext(optionsBuilder.Options);
                
                
                bool created = await context.Database.EnsureCreatedAsync();
                
                if (created)
                {
                    return (true, "? Đã tạo database và bảng thành công!");
                }
                else
                {
                    return (true, "?? Database và bảng đã tồn tại.");
                }
            }
            catch (Exception ex)
            {
                return (false, $"? Không thể tạo database:\n{ex.Message}");
            }
        }

       
        public static async Task<(bool success, string message)> SeedTestDataAsync()
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var repo = dbManager.UserRepository;
                
             
                if (!await repo.IsUsernameExistsAsync("admin"))
                {
                    await repo.RegisterUserAsync("admin", "123456", "Administrator");
                }
             
                if (!await repo.IsUsernameExistsAsync("test"))
                {
                    await repo.RegisterUserAsync("test", "test123", "Test User");
                }
                
                return (true, "? Đã thêm dữ liệu test:\n- Username: admin / Password: 123456\n- Username: test / Password: test123");
            }
            catch (Exception ex)
            {
                return (false, $"? Không thể thêm dữ liệu test: {ex.Message}");
            }
        }

    
        public static async Task<(bool success, string message)> ListAllUsersAsync()
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var context = dbManager.Context;
                
                var users = await context.AppUsers.ToListAsync();
                
                if (users.Count == 0)
                {
                    return (true, "?? Chưa có user nào trong database.");
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
                return (false, $"? Lỗi khi lấy danh sách users: {ex.Message}");
            }
        }

    
        public static async Task<(bool success, string message)> DebugLoginAsync(string username, string password)
        {
            try
            {
                using var dbManager = new DatabaseManager();
                var context = dbManager.Context;
                
                // Kiểm tra từng  users
                var totalUsers = await context.AppUsers.CountAsync();
                
                // Tìm user theo username
                var userByUsername = await context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username == username);
                
                if (userByUsername == null)
                {
                    return (false, $"? Username '{username}' KHÔNG TỒN TẠI trong database!\n\n" +
                                   $"?? Tổng số users: {totalUsers}\n\n" +
                                   $"?? Các username có sẵn:\n" +
                                   $"{await GetAllUsernamesAsync(context)}");
                }
                
                // Ki?m tra password
                var storedPassword = userByUsername.Password;
                var passwordMatch = storedPassword == password;
                
                var result = $"?? DEBUG LOGIN:\n\n" +
                            $"Username nhập: '{username}'\n" +
                            $"Password nhập: '{password}'\n\n" +
                            $"Username trong DB: '{userByUsername.Username}'\n" +
                            $"Password trong DB: '{storedPassword}'\n\n" +
                            $"? Username khớp: YES\n" +
                            $"{(passwordMatch ? "?" : "?")} Password khớp: {(passwordMatch ? "YES" : "NO")}\n\n";
                
                if (!passwordMatch)
                {
                    result += $"? MẬT KHẨU KHÔNG KHỚP!\n" +
                             $"Bạn nhâp: '{password}' (length: {password.Length})\n" +
                             $"Trong DB: '{storedPassword}' (length: {storedPassword.Length})\n\n" +
                             $"Có thể do:\n" +
                             $"• Sai mật khẩu\n" +
                             $"• Có khoảng trắng\n" +
                             $"• Caps Lock bật";
                }
                else
                {
                    result += "? Có username và password đều ĐÚNG!";
                }
                
                return (passwordMatch, result);
            }
            catch (Exception ex)
            {
                return (false, $"? Lỗi debug: {ex.Message}");
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
