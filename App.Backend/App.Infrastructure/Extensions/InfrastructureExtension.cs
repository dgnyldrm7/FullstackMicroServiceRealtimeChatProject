using App.Core.Interface;
using App.Core.Interface.RabbitMQ;
using App.Core.Interface.Redis;
using App.Infrastructure.Cache;
using App.Infrastructure.Identity;
using App.Infrastructure.JwtToken;
using App.Infrastructure.RabbitMQ;
using App.Infrastructure.Redis;
using App.Infrastructure.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;

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

            //RabbitMQ işlemleri
            services.AddSingleton<IConnectionFactory, ConnectionFactory>();

            services.AddSingleton<RabbitMQConnection>();

            services.AddScoped<IRabbitMQPublisher, RabbitMQPublisher>();

            //MemoryCache DI
            services.AddMemoryCache();

            services.AddScoped<IMemoryCacheService, MemoryCacheService>();

            //SignalR
            services.AddSingleton<IUserIdProvider, PhoneBasedUserIdProvider>();
        }
    }
}