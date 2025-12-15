using System;
using System.Threading.Tasks;
using Database.models;
using Microsoft.EntityFrameworkCore;

namespace Database.Repositories
{
    /// <summary>
    /// Repository ð? thao tác v?i User trong database
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Ki?m tra k?t n?i t?i database
        /// </summary>
        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xác th?c ngý?i dùng ðãng nh?p
        /// </summary>
        public async Task<bool> AuthenticateUserAsync(string username, string password)
        {
            try
            {
                var user = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.Password == password);
                
                return user != null;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[AuthenticateUserAsync] Error: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Ki?m tra username ð? t?n t?i chýa
        /// </summary>
        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            try
            {
                // Ki?m tra không phân bi?t hoa thý?ng
                var exists = await _context.AppUsers
                    .AnyAsync(u => u.Username.ToLower() == username.ToLower());
                
                System.Diagnostics.Debug.WriteLine($"[IsUsernameExistsAsync] Username '{username}' exists: {exists}");
                return exists;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[IsUsernameExistsAsync] Error: {ex.Message}");
                throw; // Throw exception thay v? return false
            }
        }

        /// <summary>
        /// Ðãng k? ngý?i dùng m?i
        /// </summary>
        public async Task<bool> RegisterUserAsync(string username, string password, string fullName)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] Attempting to register username: '{username}'");
                
                // Ki?m tra username ð? t?n t?i (không phân bi?t hoa thý?ng)
                var existingUser = await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
                
                if (existingUser != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] Username '{username}' already exists!");
                    return false;
                }

                System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] Username '{username}' is available, creating new user...");

                var newUser = new AppUser
                {
                    Username = username,
                    Password = password,
                    FullName = fullName
                    // Không set UserId và CreatedAt - ð? database t? ð?ng t?o
                };

                _context.AppUsers.Add(newUser);
                var saved = await _context.SaveChangesAsync();
                
                System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] SaveChangesAsync returned: {saved}");
                
                if (saved > 0)
                {
                    System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] Successfully registered user '{username}' with ID: {newUser.UserId}!");
                    return true;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] SaveChanges returned 0, no rows affected!");
                    return false;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] Exception: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[RegisterUserAsync] InnerException: {ex.InnerException.Message}");
                }
                return false;
            }
        }

        /// <summary>
        /// L?y thông tin ngý?i dùng theo username
        /// </summary>
        public async Task<AppUser?> GetUserByUsernameAsync(string username)
        {
            try
            {
                return await _context.AppUsers
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// L?y thông tin ngý?i dùng theo ID
        /// </summary>
        public async Task<AppUser?> GetUserByIdAsync(int id)
        {
            try
            {
                return await _context.AppUsers.FindAsync(id);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// C?p nh?t thông tin ngý?i dùng
        /// </summary>
        public async Task<bool> UpdateUserAsync(AppUser user)
        {
            try
            {
                _context.AppUsers.Update(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Xóa ngý?i dùng
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            try
            {
                var user = await GetUserByIdAsync(id);
                if (user == null) return false;

                _context.AppUsers.Remove(user);
                return await _context.SaveChangesAsync() > 0;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Ð?m b?o database ðý?c t?o
        /// </summary>
        public async Task<bool> EnsureDatabaseCreatedAsync()
        {
            try
            {
                return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}
