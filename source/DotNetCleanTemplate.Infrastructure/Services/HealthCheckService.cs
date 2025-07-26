using DotNetCleanTemplate.Domain.Models;
using DotNetCleanTemplate.Domain.Services;
using DotNetCleanTemplate.Infrastructure.Persistent;

namespace DotNetCleanTemplate.Infrastructure.Services;

public class HealthCheckService : IHealthCheckService
{
    private readonly AppDbContext _dbContext;
    private readonly ICacheReader _cacheReader;
    private readonly ICacheInvalidator _cacheInvalidator;

    public HealthCheckService(
        AppDbContext dbContext,
        ICacheReader cacheReader,
        ICacheInvalidator cacheInvalidator
    )
    {
        _dbContext = dbContext;
        _cacheReader = cacheReader;
        _cacheInvalidator = cacheInvalidator;
    }

    public async Task<IHealthCheckResult> CheckAsync(CancellationToken cancellationToken)
    {
        var dbStatus = HealthCheckResultStatus.Unhealthy;
        var cacheStatus = HealthCheckResultStatus.Unhealthy;

        // Проверка БД
        try
        {
            if (await _dbContext.Database.CanConnectAsync(cancellationToken))
                dbStatus = HealthCheckResultStatus.Healthy;
        }
        catch
        {
            dbStatus = HealthCheckResultStatus.Unhealthy;
        }

        // Проверка кэша (set/get)
        try
        {
            var testKey = $"healthcheck_{Guid.NewGuid()}";
            var testValue = "ok";
            var value = await _cacheReader.GetOrCreateAsync<string>(
                testKey,
                null,
                () => Task.FromResult(testValue),
                cancellationToken
            );
            if (value == testValue)
                cacheStatus = HealthCheckResultStatus.Healthy;
            _cacheInvalidator.Invalidate(testKey);
        }
        catch
        {
            cacheStatus = HealthCheckResultStatus.Unhealthy;
        }

        var status =
            dbStatus == HealthCheckResultStatus.Healthy
            && cacheStatus == HealthCheckResultStatus.Healthy
                ? HealthCheckResultStatus.Healthy
                : HealthCheckResultStatus.Degraded;

        return new HealthCheckResult
        {
            Status = status,
            DatabaseStatus = dbStatus,
            CacheStatus = cacheStatus,
            ServerTime = DateTime.UtcNow,
        };
    }
}
