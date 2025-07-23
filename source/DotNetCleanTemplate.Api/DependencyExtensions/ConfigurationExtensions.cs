using DotNetEnv;
using DotNetEnv.Configuration;
using NetEnvExtensions;

namespace DotNetCleanTemplate.Api.DependencyExtensions;

/// <summary>
/// Расширения для конфигурации приложения
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Инициализировать конфигурацию приложения
    /// </summary>
    public static WebApplicationBuilder InitializeConfiguration(
        this WebApplicationBuilder builder,
        bool isTestEnvironment = false
    )
    {
        if (isTestEnvironment)
            return builder;

        builder
            .Configuration.AddJsonFile(
                "configs/appsettings.json",
                optional: false,
                reloadOnChange: true
            )
            .AddJsonFile("configs/cache.json", optional: false, reloadOnChange: true)
            .AddJsonFile("configs/initData.json", optional: false, reloadOnChange: true)
            .AddJsonFile("configs/serilog.json", optional: false, reloadOnChange: true)
#if DEBUG
            .AddDotNetEnv(".env", LoadOptions.TraversePath())
#endif
            .AddEnvironmentVariableSubstitution();

        return builder;
    }
}
