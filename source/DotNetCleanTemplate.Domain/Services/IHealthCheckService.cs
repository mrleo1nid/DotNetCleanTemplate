using DotNetCleanTemplate.Domain.Models;

namespace DotNetCleanTemplate.Domain.Services;

public interface IHealthCheckService
{
    Task<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken);
}
