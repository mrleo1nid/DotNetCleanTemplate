using CacheManager.Core;
using DotNetCleanTemplate.Domain.Services;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICacheManager<object> _cache;

        public CacheService(ICacheManager<object> cache)
        {
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache;
        }

        public async Task<T> GetOrCreateAsync<T>(
            string key,
            string? region,
            Func<Task<T>> factory,
            CancellationToken cancellationToken
        )
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));

            ArgumentNullException.ThrowIfNull(factory);

            var item = _cache.Get<T>(key);
            if (!EqualityComparer<T>.Default.Equals(item, default))
            {
                return item;
            }

            var value = await factory();
            if (value is not null && !EqualityComparer<T>.Default.Equals(value, default))
            {
                if (!string.IsNullOrEmpty(region))
                {
                    _cache.Put(key, value, region);
                }
                else
                {
                    _cache.Put(key, value);
                }
            }
            return value;
        }

        public void Invalidate(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));

            _cache.Remove(key);
        }

        public void InvalidateRegion(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region is required.", nameof(region));
            _cache.ClearRegion(region);
        }
    }
}
