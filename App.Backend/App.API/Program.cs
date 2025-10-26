
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using App.API.Extension;
using App.API.Middlewares;
using App.Core.Interface.Redis;
using App.Core.Jwt;
using App.Infrastructure.Extensions;
using App.Logic.Extension;
using App.Logic.HubContext;
using App.Logic.Validators;
using App.Persistance.Extensions;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace App.API
{
    public class Program
    {
        [Obsolete]
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers()
                .AddFluentValidation(fv =>
                {
                    fv.RegisterValidatorsFromAssemblyContaining<UserCreateRequestValidator>();
                });

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
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Senin API Adın", Version = "v1" });

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


            //DI Container Services
            builder.Services.AddPersistanceServices(builder.Configuration);

            builder.Services.AddApiServices();

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
                    ValidIssuer = jwtSettings?.Issuer,
                    ValidAudience = jwtSettings?.Audience,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings?.Key!))
                };

                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        var redisService = context.HttpContext.RequestServices.GetRequiredService<IRedisService>();
                        var claimsIdentity = context.Principal?.Identity as ClaimsIdentity;

                        var jti = claimsIdentity?.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

                        if (string.IsNullOrEmpty(jti))
                        {
                            context.Fail("Token JTI not found");
                            return;
                        }

                        var userId = await redisService.GetAsync($"jti:{jti}");

                        if (string.IsNullOrEmpty(userId))
                        {
                            context.Fail("Token has been revoked");
                        }
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

            app.UseAuthentication();

            //JwtValidationMiddleware
            app.UseMiddleware<JwtJtiValidationMiddleware>();

            //NotFoundMiddleware
            app.UseMiddleware<NotFoundMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            //SignalR Mapping
            app.MapHub<WorkerHub>("/workerhub");

            app.Run();
        }
    }
}
