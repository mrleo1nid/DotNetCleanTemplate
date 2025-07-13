using Serilog;

namespace DotNetCleanTemplate.Api.DependencyExtensions;

/// <summary>
/// Расширения для регистрации сервисов логирования
/// </summary>
public static class LoggingServiceExtensions
{
    /// <summary>
    /// Добавить логирование с Serilog
    /// </summary>
    public static IServiceCollection AddLogging(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        // Конфигурация Serilog
        services.ConfigureSerilog(configuration, environment);

        // Регистрация Serilog
        services.AddSerilog(Log.Logger);

        return services;
    }

    /// <summary>
    /// Настроить Serilog
    /// </summary>
    public static IServiceCollection ConfigureSerilog(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment
    )
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .Enrich.WithProperty("Application", "DotNetCleanTemplate")
            .Enrich.WithProperty("Environment", environment.EnvironmentName)
            .WriteTo.Console()
            .WriteTo.File(
                path: "logs/log-.txt",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true
            )
            .CreateLogger();

        return services;
    }
}
