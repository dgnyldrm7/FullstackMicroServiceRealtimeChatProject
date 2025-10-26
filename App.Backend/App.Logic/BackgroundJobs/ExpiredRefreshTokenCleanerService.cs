using App.Core.Interface;
using App.Core.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace App.Logic.BackgroundJobs
{
    public class ExpiredRefreshTokenCleanerService : BackgroundService
    {
        private readonly ILogger<ExpiredRefreshTokenCleanerService> _logger;
        private readonly JwtSettings jwtSettings;
        private readonly IServiceScopeFactory _scopeFactory;

        public ExpiredRefreshTokenCleanerService(
            IOptions<JwtSettings> jwtOptions, 
            ILogger<ExpiredRefreshTokenCleanerService> logger, 
            IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            jwtSettings = jwtOptions.Value;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await Task.Delay(10000);

            _logger.LogInformation("Expired Refresh Token Cleaner Service is starting.");

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var removedExpiredRefreshToken = scope.ServiceProvider.GetRequiredService<IRemovedExpiredRefreshTokenService>();

                    _logger.LogInformation("Starting cleanup of expired refresh tokens.");

                    await removedExpiredRefreshToken.RemovedExpiredRefreshTokenAsync();

                    _logger.LogInformation("Cleanup completed successfully.");
                }
                catch(Exception ex)
                {
                    _logger.LogError(ex, "Error occurred during cleanup of expired tokens.");
                }
                await Task.Delay(TimeSpan.FromMinutes(jwtSettings.RefreshTokenCleanupIntervalMinutes), cancellationToken);
            }
            _logger.LogInformation("ExpiredRefreshTokenCleanerService is stopping.");
        }
    }
}