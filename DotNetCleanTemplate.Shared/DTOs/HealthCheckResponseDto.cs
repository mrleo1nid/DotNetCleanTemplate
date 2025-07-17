namespace DotNetCleanTemplate.Shared.DTOs;

public class HealthCheckResponseDto
{
    public HealthCheckResultStatus Status { get; set; }
    public HealthCheckResultStatus DatabaseStatus { get; set; }
    public HealthCheckResultStatus CacheStatus { get; set; }
    public DateTime ServerTime { get; set; }
}

public enum HealthCheckResultStatus
{
    Healthy,
    Degraded,
    Unhealthy,
}
