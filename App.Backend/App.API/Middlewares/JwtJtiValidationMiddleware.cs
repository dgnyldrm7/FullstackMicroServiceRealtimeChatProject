using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using App.Core.Interface.Redis;
using Newtonsoft.Json;

namespace App.API.Middlewares
{
    public class JwtJtiValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public JwtJtiValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IRedisService redisService)
        {
            var path = context.Request.Path.Value;

            if (path.StartsWith("/workerhub", StringComparison.OrdinalIgnoreCase) ||
                path.StartsWith("/apiworker", StringComparison.OrdinalIgnoreCase))
            {
                await _next(context);
                return;
            }


            if (IsPathExcluded(context))
            {
                await _next(context);
                return;
            }

            // Eğer Authorization header yoksa, doğrudan "missing" mesajı döndür
            if (!context.Request.Headers.ContainsKey("Authorization"))
            {
                await RespondUnauthorizedAsync(context, "Authentication token is missing.");
                return;
            }

            // Eğer kullanıcı authenticate edilmemişse
            if (!context.User.Identity?.IsAuthenticated ?? true)
            {
                await RespondUnauthorizedAsync(context, "Authentication token is invalid or expired.");
                return;
            }

            // 🔹 Eğer WorkerService ise Redis kontrolü yapılmaz
            var role = context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
            if (role == "SystemWorker")
            {
                await _next(context);
                return;
            }

            // Buraya sadece doğru authenticate olmuş token'lar düşer
            string? userId = context.User.Claims.FirstOrDefault(c =>
                                c.Type == ClaimTypes.NameIdentifier ||
                                c.Type == JwtRegisteredClaimNames.Sub)?.Value;

            string? tokenJti = context.User.Claims.FirstOrDefault(c =>
                                c.Type == JwtRegisteredClaimNames.Jti)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tokenJti))
            {
                await RespondUnauthorizedAsync(context, "Token does not contain required claims (sub or jti).");
                return;
            }

            var redisJti = await redisService.GetAsync($"user-jti:{userId}");

            if (redisJti != tokenJti)
            {
                await RespondUnauthorizedAsync(context, "Token revoked or invalid JTI.");
                return;
            }

            await _next(context);
        }

        private bool IsPathExcluded(HttpContext context)
        {
            var path = context.Request.Path.Value;
            var method = context.Request.Method;

            if (string.IsNullOrEmpty(path)) return false;

            return path.StartsWith("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/api/auth/refresh-token", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/api/auth/logout", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/api/chat/send-all-client", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/health", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("/health-ui", StringComparison.OrdinalIgnoreCase) ||
                   (path.StartsWith("/api/users", StringComparison.OrdinalIgnoreCase) && method.Equals("POST", StringComparison.OrdinalIgnoreCase));
        }

        private async Task RespondUnauthorizedAsync(HttpContext context, string message)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            context.Response.ContentType = "application/json";

            var result = new
            {
                StatusCode = 401,
                Message = message
            };

            string jsonResponse = JsonConvert.SerializeObject(result);
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}
