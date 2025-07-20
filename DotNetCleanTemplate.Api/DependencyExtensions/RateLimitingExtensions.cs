using System.Threading.RateLimiting;
using DotNetCleanTemplate.Api.Helpers;
using DotNetCleanTemplate.Infrastructure.Configurations;

namespace DotNetCleanTemplate.Api.DependencyExtensions;

/// <summary>
/// Расширения для настройки ограничения скорости запросов
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Добавить ограничение скорости запросов
    /// </summary>
    public static IServiceCollection AddRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        if (
            configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            == null
        )
            throw new InvalidOperationException($"{RateLimitingSettings.SectionName} is not set.");

        var rateLimitingSettings =
            configuration.GetSection(RateLimitingSettings.SectionName).Get<RateLimitingSettings>()
            ?? new RateLimitingSettings();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Обработчик отклоненных запросов
            options.OnRejected = async (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = (
                        (int)retryAfter.TotalSeconds
                    ).ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsync(
                    "Too many requests. Please try again later.",
                    cancellationToken
                );
            };

            // Создаем цепные ограничители
            var limiters = new List<PartitionedRateLimiter<HttpContext>>();

            // Ограничитель по IP адресу
            if (rateLimitingSettings.UseIpPartition)
            {
                limiters.Add(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var ipAddress = HttpContextHelper.GetClientIpAddress(httpContext);
                        return RateLimitPartition.GetFixedWindowLimiter(
                            $"ip:{ipAddress}",
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingSettings.IpPermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitingSettings.IpWindowSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = rateLimitingSettings.QueueLimit,
                                AutoReplenishment = true,
                            }
                        );
                    })
                );
            }

            // Ограничитель по API ключу
            if (rateLimitingSettings.UseApiKeyPartition)
            {
                limiters.Add(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        var apiKey = httpContext
                            .Request.Headers[rateLimitingSettings.ApiKeyHeaderName]
                            .FirstOrDefault();
                        var partitionKey = !string.IsNullOrEmpty(apiKey)
                            ? $"api_key:{apiKey}"
                            : "no_api_key";

                        return RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey,
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingSettings.ApiKeyPermitLimit,
                                Window = TimeSpan.FromSeconds(
                                    rateLimitingSettings.ApiKeyWindowSeconds
                                ),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = rateLimitingSettings.QueueLimit,
                                AutoReplenishment = true,
                            }
                        );
                    })
                );
            }

            // Если ни один из ограничителей не включен, создаем глобальный
            if (limiters.Count == 0)
            {
                limiters.Add(
                    PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                    {
                        return RateLimitPartition.GetFixedWindowLimiter(
                            "global",
                            _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = rateLimitingSettings.PermitLimit,
                                Window = TimeSpan.FromSeconds(rateLimitingSettings.WindowSeconds),
                                QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                                QueueLimit = rateLimitingSettings.QueueLimit,
                                AutoReplenishment = true,
                            }
                        );
                    })
                );
            }

            // Устанавливаем глобальный ограничитель как цепочку
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(limiters.ToArray());
        });

        return services;
    }
}
