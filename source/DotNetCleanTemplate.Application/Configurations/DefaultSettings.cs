namespace DotNetCleanTemplate.Application.Configurations
{
    public class DefaultSettings
    {
        public static readonly string SectionName = "DefaultSettings";
        public string? DefaultUserRole { get; set; } = "User";
        public string? DefaultAdminRole { get; set; } = "Admin";
    }
}
