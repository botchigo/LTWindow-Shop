using Microsoft.Extensions.Configuration;
using MyShop.Application.Interfaces;
using MyShop.Domain.Entities;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using MyShop.Shared.Contants;
using Microsoft.Extensions.Options;
using System.Text;

namespace MyShop.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtOption _jwt;

        public TokenService(IOptions<JwtOption> options)
        {
            _jwt = options.Value;
        }

        public string GenerateAccessToken(AppUser user, out DateTime expiredAt)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                //email,role,...
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            expiredAt = DateTime.UtcNow.AddMinutes(_jwt.ExpireMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience:  _jwt.Audience,
                claims: claims,
                expires: expiredAt,
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
