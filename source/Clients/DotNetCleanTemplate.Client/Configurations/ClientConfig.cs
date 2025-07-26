namespace DotNetCleanTemplate.Client.Configurations
{
    public class ClientConfig
    {
        public ApiSection Api { get; set; } = new();
    }

    public class ApiSection
    {
        public const string SectionName = "Api";
        public string BaseUrl { get; set; } = string.Empty;
    }
}
