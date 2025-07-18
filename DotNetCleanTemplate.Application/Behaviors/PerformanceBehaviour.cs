using System.Diagnostics;
using DotNetCleanTemplate.Application.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DotNetCleanTemplate.Application.Behaviors
{
    public class PerformanceBehaviour<TRequest, TResponse>(
        ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
        IOptions<PerformanceSettings> settings
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<PerformanceBehaviour<TRequest, TResponse>> _logger = logger;
        private readonly PerformanceSettings _settings = settings.Value;
        private readonly Stopwatch _timer = new();

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            _timer.Start();
            var response = await next();
            _timer.Stop();

            var elapsedMs = _timer.ElapsedMilliseconds;

            if (elapsedMs > _settings.LongRunningThresholdMs)
            {
                _logger.LogWarning(
                    "Long running request: {Name} ({ElapsedMs} ms) {@Request}",
                    typeof(TRequest).Name,
                    elapsedMs,
                    request
                );
            }

            return response;
        }
    }
}
