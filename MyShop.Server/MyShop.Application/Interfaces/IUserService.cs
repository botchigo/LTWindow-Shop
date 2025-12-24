using MyShop.Domain.Entities;
using MyShop.Shared.DTOs.Users;

namespace MyShop.Application.Interfaces
{
    public interface IUserService
    {
        Task<AppUser> LoginAsync(LoginDTO request);
        Task<AppUser> RegisterUserAsync(RegisterDTO request);
    }
}
