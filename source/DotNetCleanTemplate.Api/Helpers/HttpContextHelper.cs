using DotNetCleanTemplate.Infrastructure.Configurations;

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
                if (!string.IsNullOrWhiteSpace(apiKey))
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
            if (!string.IsNullOrWhiteSpace(forwardedHeader))
            {
                // Берем первый непустой IP из списка (клиентский IP)
                var ips = forwardedHeader.Split(',');
                foreach (var ip in ips)
                {
                    var trimmedIp = ip.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedIp))
                    {
                        return trimmedIp;
                    }
                }
            }

            // Проверяем заголовок X-Real-IP
            var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(realIp))
            {
                return realIp;
            }

            // Используем RemoteIpAddress как fallback
            return httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        }
    }
}
