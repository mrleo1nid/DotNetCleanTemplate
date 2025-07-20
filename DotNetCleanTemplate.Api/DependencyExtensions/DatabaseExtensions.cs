using DotNetCleanTemplate.Infrastructure.Persistent;
using Microsoft.EntityFrameworkCore;

namespace DotNetCleanTemplate.Api.DependencyExtensions;

/// <summary>
/// Расширения для настройки базы данных
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Добавить контекст базы данных
    /// </summary>
    public static IServiceCollection AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        if (configuration.GetConnectionString("DefaultConnection") == null)
            throw new InvalidOperationException(
                "Connection string 'DefaultConnection' is not set."
            );

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))
        );

        return services;
    }
}
