using App.Core.Entities;
using App.Core.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Logic.Services
{
    public class RemovedExpiredRefreshTokenService : IRemovedExpiredRefreshTokenService
    {
        private readonly UserManager<AppUser> _userManager;

        public RemovedExpiredRefreshTokenService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task RemovedExpiredRefreshTokenAsync()
        {
            List<AppUser> users = await _userManager.Users.ToListAsync();

            foreach (AppUser user in users)
            {
                string? refreshToken = await _userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

                if (!string.IsNullOrEmpty(refreshToken))
                {
                    var expirationToken = await _userManager.GetAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");

                    if (DateTime.TryParse(expirationToken, out var expirationDate))
                    {
                        if (expirationDate < DateTime.UtcNow)
                        {
                            await _userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshToken");

                            await _userManager.RemoveAuthenticationTokenAsync(user, "MyApp", "RefreshTokenExpiry");
                        }
                    }
                }
            }
        }
    }
}
