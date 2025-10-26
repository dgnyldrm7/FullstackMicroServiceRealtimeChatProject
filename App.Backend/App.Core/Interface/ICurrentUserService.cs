using App.Core.Entities;

namespace App.Core.Interface
{
    public interface ICurrentUserService
    {
        Task<AppUser?> GetLoggedInUserAsync();
    }
}