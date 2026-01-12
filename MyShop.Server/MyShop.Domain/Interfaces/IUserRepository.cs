using MyShop.Domain.Entities;

namespace MyShop.Domain.Interfaces
{
    public interface IUserRepository : IGenericRepository<AppUser>
    {
        Task<bool> TestConnectionAsync();
        Task<bool> IsUsernameExistsAsync(string username);
        Task<AppUser?> GetUserByUsernameAsync(string username);
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<bool> EnsureDatabaseCreatedAsync();
        Task<AppUser?> GetByUsernameAndPasswordAsync(string username, string password);
    }
}
