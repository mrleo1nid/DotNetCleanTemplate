namespace DotNetCleanTemplate.Domain.Services
{
    public interface ICacheService
    {
        Task<T> GetOrCreateAsync<T>(
            string key,
            string? region,
            Func<Task<T>> factory,
            CancellationToken cancellationToken
        );
        void Invalidate(string key);
        void InvalidateRegion(string region);
    }
}
