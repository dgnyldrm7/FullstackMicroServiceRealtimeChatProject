using App.Core.Interface;
using App.Persistance.Context;
using App.Persistance.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace App.Persistance.Extensions
{
    public static class PersistanceExtension
    {
        public static void AddPersistanceServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
            {
                /*
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    //sqlOptions.MigrationsAssembly("App.Persistance"); Or you can choose!;
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
                */

                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"), sqlOptions =>
                {
                    //sqlOptions.MigrationsAssembly("App.Persistance"); Or you can choose!;
                    sqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
                });
            });

            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

            services.AddScoped<MessageRepository>();
        }
    }
}
