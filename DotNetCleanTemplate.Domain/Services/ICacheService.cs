namespace DotNetCleanTemplate.Domain.Services
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(string key, string? region, Func<Task<T>> factory);
        void InvalidateAsync(string key);
        void InvalidateRegionAsync(string region);
    }
}
