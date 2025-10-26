using App.Core.Entities;
using App.Persistance.Context;
using Microsoft.AspNetCore.Identity;

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
    }
}
