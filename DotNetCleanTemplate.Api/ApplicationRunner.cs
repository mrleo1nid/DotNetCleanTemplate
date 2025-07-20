using DotNetCleanTemplate.Api.DependencyExtensions;
using DotNetCleanTemplate.Api.Handlers;
using DotNetCleanTemplate.Domain.Services;
using FastEndpoints;
using FastEndpoints.Swagger;
using Prometheus;
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
            _app.UseMetricServer();
            _app.UseCorsExtension();
            _app.UseRateLimiter();
            _app.UseMiddleware<ErrorHandlingMiddleware>();
            _app.UseAuthentication();
            _app.UseMiddleware<UserLockoutMiddleware>();
            _app.UseAuthorization();
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

        private async Task UseMigration()
        {
            // Применение миграций при старте
            using (var scope = _app.Services.CreateScope())
            {
                var migrationService =
                    scope.ServiceProvider.GetRequiredService<IMigrationService>();
                await migrationService.ApplyMigrationsIfEnabledAsync();
            }
        }

        private async Task UseInitData()
        {
            using (var scope = _app.Services.CreateScope())
            {
                var initDataService = scope.ServiceProvider.GetRequiredService<IInitDataService>();
                await initDataService.InitializeAsync();
            }
        }

        /// <summary>
        /// Запустить приложение
        /// </summary>
        public async Task RunAsync()
        {
            try
            {
                Log.Information("Starting application");
                await UseMigration();
                await UseInitData();
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
