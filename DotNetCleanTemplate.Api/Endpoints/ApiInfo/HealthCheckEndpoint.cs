using System.Threading;
using System.Threading.Tasks;
using DotNetCleanTemplate.Application.Features.Health;
using DotNetCleanTemplate.Infrastructure.Configurations;
using DotNetCleanTemplate.Shared.DTOs;
using FastEndpoints;
using MediatR;

namespace DotNetCleanTemplate.Api.Endpoints
{
    public class HealthCheckEndpoint : EndpointWithoutRequest<HealthCheckResponseDto>
    {
        private readonly IMediator _mediator;

        public HealthCheckEndpoint(IMediator mediator)
        {
            _mediator = mediator;
        }

        public override void Configure()
        {
            Get("/health");
            AllowAnonymous();
            Summary(s =>
            {
                s.Summary = "Health check endpoint for status monitoring.";
                s.Description = "Returns 200 OK if the application and dependencies are running.";
            });
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var result = await _mediator.Send(new HealthCheckQuery(), ct);
            await SendAsync(result);
        }
    }
}
