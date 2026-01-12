using MyShop.Core.Interfaces;

namespace MyShop.Infrastructure.Authentication
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string TokenKey = "AuthToken";
        private IUserSessionService _sessionService;
        private readonly ILocalSettingService _localSettingService;

        public AuthenticationService(IUserSessionService sessionService, ILocalSettingService localSettingService)
        {
            _sessionService = sessionService;
            _localSettingService = localSettingService;
        }

        public async Task<string> GetTokenAsync()
        {
            return await _localSettingService.GetSettingAsync<string>("AuthToken") ?? string.Empty;
        }

        public async Task LogoutAsync()
        {
            await _localSettingService.RemoveSettingAsync(TokenKey);
            _sessionService.ClearSession();
        }

        public async Task SetTokenAsync(string token)
        {
            await _localSettingService.SaveSettingAsync(TokenKey, token);
            _sessionService.SetSession(token);
        }

        public async Task InitializeAsync()
        {
            var token = await GetTokenAsync();
            if (!string.IsNullOrEmpty(token))
            {
                _sessionService.SetSession(token);
            }
        }
    }
}
