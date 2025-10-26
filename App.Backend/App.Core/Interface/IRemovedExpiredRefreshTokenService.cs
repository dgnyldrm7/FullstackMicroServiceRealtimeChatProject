namespace App.Core.Interface
{
    public interface IRemovedExpiredRefreshTokenService
    {
        Task RemovedExpiredRefreshTokenAsync();
    }
}