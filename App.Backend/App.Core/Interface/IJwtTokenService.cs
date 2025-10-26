using App.Core.Jwt;

namespace App.Core.Interface
{
    public interface IJwtTokenService
    {
        ResultTokenMessage GenerateToken(string userNumber, string userName, IList<string> roles);
        (string Token, string Jti) GenerateAccessToken(string userId, string userNumber, string username, IList<string> roles);
        string GenerateRefreshToken();
    }
}