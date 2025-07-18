namespace DotNetCleanTemplate.Infrastructure.Configurations
{
    public class RateLimitingSettings
    {
        public const string SectionName = "RateLimiting";
        public const string PolicyName = "DefaultRateLimiter";

        // Общие настройки
        public int QueueLimit { get; set; } = 2;

        // Настройки для партиций
        public bool UseIpPartition { get; set; } = true;
        public bool UseApiKeyPartition { get; set; } = false;
        public string ApiKeyHeaderName { get; set; } = "X-API-Key";

        // Настройки для IP ограничителя
        public int IpPermitLimit { get; set; } = 4;
        public int IpWindowSeconds { get; set; } = 12;

        // Настройки для API ключа ограничителя
        public int ApiKeyPermitLimit { get; set; } = 20;
        public int ApiKeyWindowSeconds { get; set; } = 30;

        // Обратная совместимость
        public int PermitLimit
        {
            get => IpPermitLimit;
            set => IpPermitLimit = value;
        }

        public int WindowSeconds
        {
            get => IpWindowSeconds;
            set => IpWindowSeconds = value;
        }
    }
}
