using DotNetCleanTemplate.Infrastructure.Configurations;
using Microsoft.AspNetCore.Http;

namespace DotNetCleanTemplate.Api.Helpers
{
    public static class HttpContextHelper
    {
        public static string GetPartitionKey(HttpContext httpContext, RateLimitingSettings settings)
        {
            var partitionKey = string.Empty;

            // Приоритет API ключу, если он включен и присутствует
            if (settings.UseApiKeyPartition)
            {
                var apiKey = httpContext
                    .Request.Headers[settings.ApiKeyHeaderName]
                    .FirstOrDefault();
                if (!string.IsNullOrEmpty(apiKey))
                {
                    partitionKey = $"api_key:{apiKey}";
                }
            }

            // Если API ключ не найден или не включен, используем IP
            if (string.IsNullOrEmpty(partitionKey) && settings.UseIpPartition)
            {
                var ipAddress = GetClientIpAddress(httpContext);
                partitionKey = $"ip:{ipAddress}";
            }

            // Если ни один из методов не включен, используем общий ключ
            if (string.IsNullOrEmpty(partitionKey))
            {
                partitionKey = "global";
            }

            return partitionKey;
        }

        public static string GetClientIpAddress(HttpContext httpContext)
        {
            // Проверяем заголовки прокси
            var forwardedHeader = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (!string.IsNullOrEmpty(forwardedHeader))
            {
                // Берем первый IP из списка (клиентский IP)
                var firstIp = forwardedHeader.Split(',')[0].Trim();
                if (!string.IsNullOrEmpty(firstIp))
                {
                    return firstIp;
                }
            }

            // Проверяем заголовок X-Real-IP
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrEmpty(realIp))
            {
                return realIp;
            }

            // Используем RemoteIpAddress как fallback
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
