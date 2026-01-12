using MyShop.Domain.Entities;

namespace MyShop.Application.Commons
{
    public record LoginResponse
    {
        public AppUser User { get; init; }
        public string AccessToken { get; init; } = string.Empty;
        public DateTime ExpiredAt { get; init; }
    }
}
