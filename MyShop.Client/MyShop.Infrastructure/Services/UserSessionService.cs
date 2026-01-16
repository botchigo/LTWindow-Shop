using MyShop.Core.Interfaces;
using System.IdentityModel.Tokens.Jwt;

namespace MyShop.Infrastructure.Services
{
    public class UserSessionService : IUserSessionService
    {
        public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);

        public int UserId { get; private set; }
        public string? UserName { get; private set; }
        public string? AccessToken { get; private set; } = null;

        public void ClearSession()
        {
            AccessToken = null;
            UserId = 0;
            UserName = null;
        }

        public void SetSession(string token)
        {
            AccessToken = token;

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);

                    var idClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == JwtRegisteredClaimNames.Sub);
                    if (idClaim != null && int.TryParse(idClaim.Value, out int id))
                    {
                        UserId = id;
                    }

                    var nameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "unique_name" || c.Type == JwtRegisteredClaimNames.UniqueName);
                    if (nameClaim != null)
                    {
                        UserName = nameClaim.Value;
                    }
                }
            }
            catch
            {
                ClearSession();
            }
        }
    }
}
