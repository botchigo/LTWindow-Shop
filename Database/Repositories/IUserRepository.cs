using System.Threading.Tasks;
using Database.models;

namespace Database.Repositories
{
    public interface IUserRepository
    {
        Task<bool> TestConnectionAsync();
        Task<bool> AuthenticateUserAsync(string username, string password);
        Task<bool> IsUsernameExistsAsync(string username);
        Task<bool> RegisterUserAsync(string username, string password, string fullName);
        Task<AppUser?> GetUserByUsernameAsync(string username);
        Task<AppUser?> GetUserByIdAsync(int id);
        Task<bool> UpdateUserAsync(AppUser user);
        Task<bool> DeleteUserAsync(int id);
    }
}
