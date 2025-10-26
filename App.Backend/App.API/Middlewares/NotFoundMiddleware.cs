using Newtonsoft.Json;

namespace App.API.Middlewares
{
    public class NotFoundMiddleware
    {
        private readonly RequestDelegate _next;

        public NotFoundMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await _next(context);

            if (context.Response.StatusCode == 404 && !context.Response.HasStarted)
            {
                context.Response.ContentType = "application/json";

                var result = new
                {
                    StatusCode = 404,
                    Message = "The requested resource could not be found."
                };

                string jsonResponse = JsonConvert.SerializeObject(result);

                await context.Response.WriteAsync(jsonResponse);
            }
        }
    }
}
