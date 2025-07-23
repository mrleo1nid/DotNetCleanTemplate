using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Shared.DTOs;
using MediatR;

namespace DotNetCleanTemplate.Application.Features.Health;

public class HealthCheckQueryHandler : IRequestHandler<HealthCheckQuery, HealthCheckResponseDto>
{
    private readonly IHealthCheckService _healthCheckService;

    public HealthCheckQueryHandler(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public async Task<HealthCheckResponseDto> Handle(
        HealthCheckQuery request,
        CancellationToken cancellationToken
    )
    {
        var result = await _healthCheckService.CheckAsync(cancellationToken);
        return new HealthCheckResponseDto
        {
            Status = (HealthCheckResultStatus)result.Status,
            DatabaseStatus = (HealthCheckResultStatus)result.DatabaseStatus,
            CacheStatus = (HealthCheckResultStatus)result.CacheStatus,
            ServerTime = result.ServerTime,
        };
    }
}
