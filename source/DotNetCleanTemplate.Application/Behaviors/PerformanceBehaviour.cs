using DotNetCleanTemplate.Application.Configurations;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Prometheus;
using System.Diagnostics;

namespace DotNetCleanTemplate.Application.Behaviors
{
    public class PerformanceBehaviour<TRequest, TResponse>(
        ILogger<PerformanceBehaviour<TRequest, TResponse>> logger,
        IOptions<PerformanceSettings> settings
    ) : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
    {
        private static readonly Counter LongRunningRequests = Metrics.CreateCounter(
            "long_running_requests_total",
            "Total number of long running MediatR requests",
            new CounterConfiguration { LabelNames = new[] { "request_type" } }
        );
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
            var response = await next(cancellationToken);
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
                LongRunningRequests.WithLabels(typeof(TRequest).Name).Inc();
            }

            return response;
        }
    }
}
