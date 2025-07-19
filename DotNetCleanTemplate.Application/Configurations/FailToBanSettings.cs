namespace DotNetCleanTemplate.Application.Configurations
{
    public class FailToBanSettings
    {
        public const string SectionName = "FailToBan";

        public bool EnableFailToBan { get; set; } = true;
        public int MaxFailedAttempts { get; set; } = 5;
        public int LockoutDurationMinutes { get; set; } = 15;
        public int ResetFailedAttemptsAfterMinutes { get; set; } = 30;
    }
}
