using App.Core.Jwt;

namespace App.Core.Interface
{
    public interface IRefreshTokenRepository
    {
        Task SaveRefreshTokenAsync(string userNumber, string token, DateTime expires);
        Task<RefreshToken?> GetByTokenAsync(string token, string userNumber);
        Task DeleteAsync(string token);
    }
}