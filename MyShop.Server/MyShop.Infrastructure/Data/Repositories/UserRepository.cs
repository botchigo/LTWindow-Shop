using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyShop.Domain.Entities;
using MyShop.Domain.Interfaces;

namespace MyShop.Infrastructure.Data.Repositories
{

    public class UserRepository : GenericRepository<AppUser>, IUserRepository
    {
        public UserRepository(AppDbContext context, ILogger<UserRepository> logger) : base(context, logger)
        {
        }

        public async Task<AppUser?> GetByUsernameAndPasswordAsync(string username, string password)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Username == username && u.Password == password);
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                await EnsureDatabaseCreatedAsync();
                return await _context.Database.CanConnectAsync();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[DB Connection Error]: {ex.Message}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"[DB Inner Error]: {ex.InnerException.Message}");
                }

                return false;
            }
        }       


        public async Task<bool> IsUsernameExistsAsync(string username)
        {
            return await _dbSet.AnyAsync(u => u.Username == username);
        }       

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

        public async Task<bool> EnsureDatabaseCreatedAsync()
        {
            try
            {
                await _context.Database.EnsureCreatedAsync();
                return true;
                //return await _context.Database.CanConnectAsync();
            }
            catch
            {
                return false;
            }
        }
    }
}
