using System.Net;
using System.Text.Json;

namespace DotNetCleanTemplate.Api.Handlers
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;
        private readonly IWebHostEnvironment _env;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger,
            IWebHostEnvironment env // внедряем окружение
        )
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                var isDev = _env.EnvironmentName == "Development";
                object errorResponse = isDev
                    ? new { error = "An unexpected error occurred.", details = ex.Message }
                    : new { error = "An unexpected error occurred." };

                var result = JsonSerializer.Serialize(errorResponse);

                await context.Response.WriteAsync(result);
            }
        }
    }
}
