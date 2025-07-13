using FastEndpoints;
using FastEndpoints.Swagger;
using DotNetCleanTemplate.Api.Handlers;
using Serilog;

namespace DotNetCleanTemplate.Api
{
    public class ApplicationRunner
    {
        private readonly WebApplication _app;

        public ApplicationRunner(WebApplication app)
        {
            _app = app;
        }

        /// <summary>
        /// Настроить middleware pipeline
        /// </summary>
        public ApplicationRunner ConfigureMiddleware()
        {
            _app.UseCors();
            _app.UseMiddleware<ErrorHandlingMiddleware>();
            _app.UseFastEndpoints().UseSwaggerGen();
            return this;
        }

        public ApplicationRunner MapEndpoints()
        {
            _app.Map(
                "/error",
                (HttpContext context) =>
                {
                    return Results.Problem("An unexpected error occurred.");
                }
            );
            return this;
        }

        /// <summary>
        /// Запустить приложение
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                Log.Information("Starting application");
                await _app.RunAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(
                    ex,
                    "Application terminated unexpectedly with error: {ErrorMessage}",
                    ex.Message
                );
                throw new InvalidOperationException("Failed to start API", ex);
            }
            finally
            {
                await Log.CloseAndFlushAsync();
            }
        }
    }
}
