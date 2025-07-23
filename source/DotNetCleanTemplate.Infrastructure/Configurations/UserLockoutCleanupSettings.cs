namespace DotNetCleanTemplate.Infrastructure.Configurations;

public class UserLockoutCleanupSettings
{
    public const string SectionName = "UserLockoutCleanup";

    public int CleanupIntervalMinutes { get; set; } = 60; // Интервал очистки в минутах
    public int BatchSize { get; set; } = 100; // Размер пакета для обработки
    public bool Enabled { get; set; } = true; // Включен ли сервис
}
