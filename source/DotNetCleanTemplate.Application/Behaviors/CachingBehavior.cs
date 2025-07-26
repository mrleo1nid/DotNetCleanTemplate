using DotNetCleanTemplate.Application.Caching;
using DotNetCleanTemplate.Domain.Services;
using MediatR;
using System.Reflection;

namespace DotNetCleanTemplate.Application.Behaviors
{
    public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ICacheService _cacheService;

        public CachingBehavior(ICacheService cacheService)
        {
            _cacheService = cacheService;
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
                    _cacheService.InvalidateRegion(invalidateAttr.Region);
                else if (!string.IsNullOrEmpty(invalidateAttr.Key))
                    _cacheService.Invalidate(invalidateAttr.Key);
            }

            if (cacheAttr != null)
            {
                var key = cacheAttr.Key;
                var region = cacheAttr.Region;
                return await _cacheService.GetOrCreateAsync<TResponse>(
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
