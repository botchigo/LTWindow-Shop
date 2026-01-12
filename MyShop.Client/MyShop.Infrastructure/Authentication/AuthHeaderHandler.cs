using MyShop.Core.Interfaces;
using System.Net.Http.Headers;

namespace MyShop.Infrastructure.Authentication
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly IAuthenticationService _authenticationService;

        public AuthHeaderHandler(IAuthenticationService authenticationService)
        {
            _authenticationService = authenticationService;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var token = await _authenticationService.GetTokenAsync();

            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
