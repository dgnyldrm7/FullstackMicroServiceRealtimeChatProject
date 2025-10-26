using App.Core.Entities;
using App.Core.Options;
using App.Persistance.Context;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace App.API.Extension
{
    public static class ApiExtension
    {
        public static void AddApiServices(this IServiceCollection services)
        {
            services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<UserManager<AppUser>>();

            services.AddScoped<SignInManager<AppUser>>();

            services.AddScoped<RoleManager<IdentityRole>>();

            services.AddHttpContextAccessor();

            //SignalR Configuration
            services.AddSignalR();
        }


        public static void AddHealtyCheckService(this IServiceCollection services, WebApplicationBuilder builder)
        {
            // 🔹 Health Checks
            var connStrings = builder.Configuration.GetSection("ConnectionStrings").Get<ConnectionStringsOptions>();
            var connStringsRabbitMQ = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMQOptions>();

            builder.Services.AddHealthChecks()
                .AddSqlServer(connStrings!.DefaultConnection, name: "SQL Server", tags: new[] { "db" })
                .AddRedis(connStrings.RedisConnection, name: "Redis", tags: new[] { "cache" });

            // 🔹 HealthChecks UI
            builder.Services.AddHealthChecksUI(options =>
            {
                options.SetEvaluationTimeInSeconds(15);
                options.MaximumHistoryEntriesPerEndpoint(50);
                options.AddHealthCheckEndpoint("API Health", "/health");
            }).AddInMemoryStorage();


        }
    }
}
