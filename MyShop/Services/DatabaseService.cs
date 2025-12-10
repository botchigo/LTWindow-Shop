using System;
using System.Threading.Tasks;
using Npgsql;

namespace MyShop.Services
{
    /// <summary>
    /// Service để kết nối và thao tác với PostgreSQL database
    /// </summary>
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            // Cấu hình connection string - thay đổi theo thông tin server của bạn
            _connectionString = BuildConnectionString(
                host: "localhost",      // Địa chỉ server PostgreSQL
                port: 5432,             // Port mặc định của PostgreSQL
                database: "MyShop",     // Tên database
                username: "postgres",   // Username
                password: "12345" // Password - thay bằng password thực của bạn
            );
        }

        public DatabaseService(string host, int port, string database, string username, string password)
        {
            _connectionString = BuildConnectionString(host, port, database, username, password);
        }

        private static string BuildConnectionString(string host, int port, string database, string username, string password)
        {
            return $"Host={host};Port={port};Database={database};Username={username};Password={password}";
        }

        /// <summary>
        /// Kiểm tra kết nối tới PostgreSQL server
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await using var connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Lấy kết nối tới database
        /// </summary>
        public async Task<NpgsqlConnection> GetConnectionAsync()
        {
            var connection = new NpgsqlConnection(_connectionString);
            await connection.OpenAsync();
            return connection;
        }

        /// <summary>
        /// Xác thực người dùng đăng nhập
        /// </summary>
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                await using var connection = await GetConnectionAsync();
                await using var command = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM app_user WHERE username = @username AND password = @password",
                    connection);
                
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password); 
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Kiểm tra username đã tồn tại chưa
        /// </summary>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                await using var connection = await GetConnectionAsync();
                await using var command = new NpgsqlCommand(
                    "SELECT COUNT(*) FROM app_user WHERE username = @username",
                    connection);
                
                command.Parameters.AddWithValue("@username", username);
                
                var result = await command.ExecuteScalarAsync();
                return Convert.ToInt32(result) > 0;
            }
            catch (Exception)
            {
                return true; // Trả về true để tránh đăng ký khi có lỗi
            }
        }

        /// <summary>
        /// Đăng ký người dùng mới
        /// </summary>
        public async Task<bool> RegisterUserAsync(string username, string password, string name)
        {
            try
            {
                // Kiểm tra username đã tồn tại
                if (await IsUsernameExistsAsync(username))
                {
                    return false;
                }

                await using var connection = await GetConnectionAsync();
                await using var command = new NpgsqlCommand(
                    "INSERT INTO app_user (username, password, full_name) VALUES (@username, @password, @full_name)",
                    connection);
                
                command.Parameters.AddWithValue("@username", username);
                command.Parameters.AddWithValue("@password", password);
                command.Parameters.AddWithValue("@full_name", name);
                
                var rowsAffected = await command.ExecuteNonQueryAsync();
                return rowsAffected > 0;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
