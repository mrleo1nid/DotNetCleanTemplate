using MediatR;
using Prometheus;

namespace DotNetCleanTemplate.Application.Behaviors
{
    public class MetricsBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private static readonly Counter RequestCounter = Metrics.CreateCounter(
            "mediatr_requests_total",
            "Total number of MediatR requests",
            new CounterConfiguration { LabelNames = new[] { "request_type" } }
        );

        private static readonly Histogram RequestDuration = Metrics.CreateHistogram(
            "mediatr_request_duration_seconds",
            "Request duration in seconds",
            new HistogramConfiguration { LabelNames = new[] { "request_type" } }
        );

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken
        )
        {
            var requestType = typeof(TRequest).Name;
            RequestCounter.WithLabels(requestType).Inc();

            using (RequestDuration.WithLabels(requestType).NewTimer())
            {
                return await next(cancellationToken);
            }
        }
    }
}
