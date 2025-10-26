using App.Core.Interface;
using App.Logic.BackgroundJobs;
using App.Logic.Services;
using App.Logic.UoW;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace App.Logic.Extension
{
    public static class LogicExtension
    {
        public static void AddLogicServices(this IServiceCollection services, IConfiguration builder)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<AuthService>();

            services.AddScoped<UserService>();

            services.AddScoped<MessageService>();

            services.AddScoped<ConversationService>();

            services.AddScoped<FriendService>();

            services.AddScoped<IRemovedExpiredRefreshTokenService, RemovedExpiredRefreshTokenService>();

            services.AddHostedService<ExpiredRefreshTokenCleanerService>();

            services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.GetConnectionString("RedisConnection")!));
        }
    }
}
