using App.API.Extension;
using App.API.Middlewares;
using App.Core.Interface.Redis;
using App.Core.Options;
using App.Infrastructure.Extensions;
using App.Logic.Extension;
using App.Logic.HubContext;
using App.Logic.Validators;
using App.Persistance.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace App.API
{
    public class Program
    {
        [Obsolete]
        public static void Main(string[] args)
        {
            // Serilog konfigürasyonu, builder'dan önce olmalı
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .Build())
                .Enrich.FromLogContext()
                .CreateLogger();

            var builder = WebApplication.CreateBuilder(args);


            builder.Host.UseSerilog();

            builder.Services.AddControllers();

            builder.Services.AddFluentValidationAutoValidation();
            
            builder.Services.AddFluentValidationClientsideAdapters();
            
            builder.Services.AddValidatorsFromAssemblyContaining<UserCreateRequestValidator>();

            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(ms => ms.Value?.Errors.Count > 0)
                        .Select(ms => new
                        {
                            field = ms.Key,
                            errors = ms.Value!.Errors.Select(e => e.ErrorMessage).ToArray()
                        })
                        .ToArray();

                    var responseObj = new
                    {
                        statusCode = 400,
                        message = "Validation errors occurred.",
                        errors = errors
                    };

                    return new BadRequestObjectResult(responseObj);
                };
            });

            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "RealtimChatApplication", Version = "v1" });

                // JWT Auth ayarları
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT token'ınızı şu formatta girin: Bearer <token>"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // Health Checks(Redis, SqlServer, Seq, RabbitMQ) - Option Pattern
            builder.Services.Configure<ConnectionStringsOptions>(
            builder.Configuration.GetSection("ConnectionStrings"));
            builder.Services.Configure<JwtSettings>(
                builder.Configuration.GetSection("JwtSettings"));
            builder.Services.Configure<WorkerAuthOptions>(
                builder.Configuration.GetSection("WorkerAuth"));
            builder.Services.Configure<RabbitMQOptions>(
                builder.Configuration.GetSection("RabbitMQ"));


            //DI Container Services
            builder.Services.AddPersistanceServices(builder.Configuration);

            builder.Services.AddApiServices();

            builder.Services.AddHealtyCheckService(builder);


            builder.Services.AddLogicServices(builder.Configuration);

            builder.Services.AddInfrastructureServices();

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            //HttpOnly Cookie için gerekli olan HttpContextAccessor servisini ekliyoruz
            builder.Services.AddHttpContextAccessor();           

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowMyLocalHost", policy =>
                    policy.WithOrigins("http://localhost:4200")
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials()
                );
            });

            builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    // 🔹 1️⃣ SignalR bağlantısında access_token query’den alınır
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/workerhub"))
                        {
                            context.Token = accessToken;
                        }
                        return Task.CompletedTask;
                    },

                    // Worker Service token'ları Redis kontrolünden muaf
                    OnTokenValidated = async context =>
                    {
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;
                        var role = claimsIdentity?.FindFirst(ClaimTypes.Role)?.Value;

                        if (role == "SystemWorker")
                            return; // Worker token'ı -> Redis kontrolü yok

                        var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
                        var jti = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (string.IsNullOrEmpty(jti))
                        {
                            context.Fail("Token JTI not found");
                            return;
                        }

                        var userId = await redisService.GetAsync($"jti:{jti}");
                        if (string.IsNullOrEmpty(userId))
                            context.Fail("Token has been revoked");
                    }
                };
            });


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }


            app.UseCors("AllowMyLocalHost");

            app.UseHttpsRedirection();

            app.UseStaticFiles(); // HealthChecks UI'nin frontend dosyaları için GEREKLİ

            // Health endpoint'leri
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
            app.MapHealthChecksUI(options =>
            {
                options.UIPath = "/health-ui";
            });

            app.UseMiddleware<JwtCookieMiddleware>();

            app.UseAuthentication();

            app.UseMiddleware<JwtJtiValidationMiddleware>();

            app.UseMiddleware<NotFoundMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            //SignalR Mapping
            app.MapHub<WorkerHub>("/workerhub");

            app.Run();
        }
    }
}
