using System.Reflection;
using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Domain.Services;
using MediatR;

namespace DotNetCleanTemplate.Application.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICacheReader _cacheReader;
        private readonly ICacheInvalidator _cacheInvalidator;

        public CachingBehavior(ICacheReader cacheReader, ICacheInvalidator cacheInvalidator)
        {
            _cacheReader = cacheReader;
            _cacheInvalidator = cacheInvalidator;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var cacheAttr = typeof(TRequest).GetCustomAttribute<CacheAttribute>();
            var invalidateAttr = typeof(TRequest).GetCustomAttribute<InvalidateCacheAttribute>();

            if (invalidateAttr != null)
            {
                if (!string.IsNullOrEmpty(invalidateAttr.Region))
                    _cacheInvalidator.InvalidateRegion(invalidateAttr.Region);
                else if (!string.IsNullOrEmpty(invalidateAttr.Key))
                    _cacheInvalidator.Invalidate(invalidateAttr.Key);
            }

            if (cacheAttr != null)
            {
                var key = cacheAttr.Key;
                var region = cacheAttr.Region;
                return await _cacheReader.GetOrCreateAsync<TResponse>(
                    key,
                    region,
                    () => next(cancellationToken),
                    cancellationToken
                );
            }

            return await next(cancellationToken);
        }
    }
}
