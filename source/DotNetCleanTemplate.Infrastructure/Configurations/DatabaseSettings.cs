namespace DotNetCleanTemplate.Infrastructure.Configurations
{
    public class DatabaseSettings
    {
        public const string SectionName = "Database";

        public bool ApplyMigrationsOnStartup { get; set; } = true;
    }
}
