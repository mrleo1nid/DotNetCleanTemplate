namespace DotNetCleanTemplate.Application.Caching
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
    public class InvalidateCacheAttribute : Attribute
    {
        public string? Key { get; set; }
        public string? Region { get; set; }

        public InvalidateCacheAttribute(string? key = null, string? region = null)
        {
            Key = key;
            Region = region;
        }
    }
}
