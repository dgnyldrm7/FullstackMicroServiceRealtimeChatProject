using System.Security.Claims;
using App.Core.Entities;
using App.Core.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace App.Infrastructure.Identity
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        public CurrentUserService(IHttpContextAccessor _httpContextAccessor, UserManager<AppUser> userManager)
        {
            this._httpContextAccessor = _httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<AppUser?> GetLoggedInUserAsync()
        {
            string? loggedInUserPhoneNumber = _httpContextAccessor.HttpContext?.User?.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.MobilePhone)?.Value;

            if (string.IsNullOrEmpty(loggedInUserPhoneNumber))
            {
                return null;
            }

            AppUser? user = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == loggedInUserPhoneNumber);

            if (user == null)
            {
                return null;
            }

            return user;
        }
    }
}
