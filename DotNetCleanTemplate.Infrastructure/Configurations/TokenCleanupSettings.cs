namespace DotNetCleanTemplate.Infrastructure.Configurations
{
    public class TokenCleanupSettings
    {
        public const string SectionName = "TokenCleanup";

        public int CleanupIntervalHours { get; set; } = 1;
        public int RetryDelayMinutes { get; set; } = 5;
        public bool EnableCleanup { get; set; } = true;
    }
}
