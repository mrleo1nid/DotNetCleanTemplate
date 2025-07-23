namespace DotNetCleanTemplate.Application.Configurations
{
    public class PerformanceSettings
    {
        public const string SectionName = "Performance";

        public int LongRunningThresholdMs { get; set; } = 500;
    }
}
