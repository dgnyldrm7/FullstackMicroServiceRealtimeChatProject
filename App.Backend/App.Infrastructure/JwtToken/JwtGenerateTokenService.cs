using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using App.Core.Entities;
using App.Core.Interface;
using App.Core.Jwt;
using App.Core.Options;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;


namespace App.Infrastructure.JwtToken
{
    public class JwtGenerateTokenService : IJwtTokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<AppUser> userManager;

        public JwtGenerateTokenService(IOptions<JwtSettings> jwtSettings, UserManager<AppUser> userManager)
        {
            _jwtSettings = jwtSettings.Value;
            this.userManager = userManager;
        }

        public ResultTokenMessage GenerateToken(string userNumber, string username, IList<string> roles)
        {
            var user = userManager.Users.FirstOrDefault(x => x.PhoneNumber == userNumber);

            if (user == null)
            {
                return new ResultTokenMessage
                {
                    Message = "Failed",
                };
            }

            var accessToken = GenerateAccessToken(user.Id, userNumber, username, roles);

            string refreshToken = GenerateRefreshToken();

            return new ResultTokenMessage
            {
                Message = "Token generated successfully",
                AccessToken = accessToken.Token,
                RefreshToken = refreshToken
            };
        }

        public (string Token, string Jti) GenerateAccessToken(string userId, string userNumber, string userName, IList<string> roles)
        {
            string jti = Guid.NewGuid().ToString();

            var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, jti),
            new Claim(ClaimTypes.MobilePhone, userNumber)
        };

            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes),
                signingCredentials: creds);

            return (new JwtSecurityTokenHandler().WriteToken(token), jti);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

    }
}
