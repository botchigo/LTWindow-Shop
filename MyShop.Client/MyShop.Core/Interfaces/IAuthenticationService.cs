namespace MyShop.Core.Interfaces
{
    public interface IAuthenticationService
    {
        Task<string> GetTokenAsync();
        Task SetTokenAsync(string token);
        Task LogoutAsync();
        Task InitializeAsync();
    }
}
