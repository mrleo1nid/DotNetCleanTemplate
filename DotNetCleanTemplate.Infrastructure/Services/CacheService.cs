using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using CacheManager.Core.Configuration;
using DotNetCleanTemplate.Domain.Services;

namespace DotNetCleanTemplate.Infrastructure.Services
{
    public class CacheService : ICacheService
    {
        private readonly ICache<object> _cache;

        public CacheService(ICache<object> cache)
        {
            ArgumentNullException.ThrowIfNull(cache);
            _cache = cache;
        }

        public async Task<T> GetOrCreateAsync<T>(string key, string? region, Func<Task<T>> factory)
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

        public void InvalidateAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("Key is required.", nameof(key));

            _cache.Remove(key);
        }

        public void InvalidateRegionAsync(string region)
        {
            if (string.IsNullOrWhiteSpace(region))
                throw new ArgumentException("Region is required.", nameof(region));
            _cache.ClearRegion(region);
        }
    }
}
