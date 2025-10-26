using App.Core.Entities;
using App.Core.Interface;
using App.Core.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Persistance.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly UserManager<AppUser> userManager;

        public RefreshTokenRepository(UserManager<AppUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task SaveRefreshTokenAsync(string userNumber, string refreshToken, DateTime expires)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);

            if (user == null)
                throw new Exception("User not found");

            // Daha önce bir refresh token varsa ve süresi geçmemişse, yenisini kaydetme (opsiyonel kontrol)
            var existingToken = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

            var expiryString = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");

            if (!string.IsNullOrEmpty(existingToken) && !string.IsNullOrEmpty(expiryString))
            {
                if (DateTime.TryParse(expiryString, out var existingExpiry))
                {
                    if (existingExpiry > DateTime.UtcNow)
                        return; // Token hala geçerli, tekrar oluşturma
                }

                // Süresi geçmişse sil
                await userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");
                await userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");
            }

            // Yeni refresh token’ı kaydet
            await userManager.SetAuthenticationTokenAsync(user, "MyApp", "RefreshToken", refreshToken);
            await userManager.SetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry", expires.ToString("O"));
        }


        public async Task<RefreshToken?> GetByTokenAsync(string token, string userNumber)
        {
            var user = await userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == userNumber);

            if (user == null)
                return null;

            var savedToken = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

            var expiry = await userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");

            if (savedToken != token)
                return null;

            if (!DateTime.TryParse(expiry, out var expiryDate) || expiryDate < DateTime.UtcNow)
                return null;

            return new RefreshToken
            {
                Token = token,
                UserNumber = userNumber,
                Expires = expiryDate
            };
        }




        public async Task DeleteAsync(string refreshToken)
        {
            // Tüm kullanıcıları getir (uygun değil büyük sistemlerde ama örnek için)
            var users = await userManager.Users.ToListAsync();

            AppUser? user = null;

            foreach (var u in users)
            {
                var token = await userManager.GetAuthenticationTokenAsync(u, "MyApp", "RefreshToken");
                if (token == refreshToken)
                {
                    user = u;
                    break;
                }
            }

            if (user == null)
                return;

            await userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");
            await userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");
        }


    }

}
