using MyShop.Domain.Entities;

namespace MyShop.Application.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(AppUser user, out DateTime expiredAt);
    }
}
