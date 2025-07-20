using Serilog;

namespace DotNetCleanTemplate.Api.DependencyExtensions;

/// <summary>
/// Расширения для логирования конфигурации
/// </summary>
public static class ConfigurationLoggingExtensions
{
    /// <summary>
    /// Вывести всю конфигурацию приложения
    /// </summary>
    public static void LogConfiguration(this IConfiguration configuration)
    {
        Log.Information("=== Application Configuration ===");

        // Выводим все секции конфигурации
        foreach (var section in configuration.GetChildren())
        {
            Log.Information("Section: {SectionName}", section.Key);
            LogSectionValues(section, 1);
        }

        Log.Information("=== End Configuration ===");
    }

    /// <summary>
    /// Рекурсивно выводит значения секции конфигурации
    /// </summary>
    private static void LogSectionValues(IConfigurationSection section, int indentLevel)
    {
        var indent = new string(' ', indentLevel * 2);

        foreach (var child in section.GetChildren())
        {
            if (child.Value != null)
            {
                Log.Information("{Indent}{Key}: {Value}", indent, child.Key, child.Value);
            }
            else
            {
                Log.Information("{Indent}{Key}:", indent, child.Key);
                LogSectionValues(child, indentLevel + 1);
            }
        }
    }
}
