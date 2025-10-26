using App.Core.Interface;
using App.Core.Interface.Redis;
using App.Infrastructure.Identity;
using App.Infrastructure.JwtToken;
using App.Infrastructure.Redis;
using App.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Infrastructure.Extensions
{
    public static class InfrastructureExtension
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddScoped<IJwtTokenService, JwtGenerateTokenService>();

            services.AddScoped<IRedisService, RedisService>();

            services.AddSingleton<IUserIdProvider, PhoneBasedUserIdProvider>();

            services.AddScoped<ICurrentUserService, CurrentUserService>();
        }
    }
}