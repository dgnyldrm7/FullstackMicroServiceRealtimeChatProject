namespace App.API.Middlewares
{
    public class JwtCookieMiddleware
    {

        private readonly RequestDelegate _next;
        private readonly ILogger<JwtCookieMiddleware> _logger;

        public JwtCookieMiddleware(RequestDelegate next, ILogger<JwtCookieMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Check if the request has a cookie named "AccessToken"
            if (context.Request.Cookies.ContainsKey("AccessToken"))
            {
                // Get the value of the cookie
                var token = context.Request.Cookies["AccessToken"];

                // Set the Authorization header with the token
                context.Request.Headers["Authorization"] = $"Bearer {token}";
            }

            // Call the next middleware in the pipeline
            await _next(context);
        }
    }
}
