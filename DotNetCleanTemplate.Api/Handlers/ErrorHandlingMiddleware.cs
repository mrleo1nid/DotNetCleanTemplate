using System.Net;
using System.Text.Json;

namespace DotNetCleanTemplate.Api.Handlers
{
    public class ErrorHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ErrorHandlingMiddleware> _logger;

        public ErrorHandlingMiddleware(
            RequestDelegate next,
            ILogger<ErrorHandlingMiddleware> logger
        )
        {
            _next = next;
            _logger = logger;
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

                var result = JsonSerializer.Serialize(
                    new
                    {
                        error = "An unexpected error occurred.",
                        details = ex.Message, // В проде лучше не возвращать детали!
                    }
                );

                await context.Response.WriteAsync(result);
            }
        }
    }
}
