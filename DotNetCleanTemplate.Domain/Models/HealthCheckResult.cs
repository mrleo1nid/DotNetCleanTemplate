using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCleanTemplate.Domain.Models
{
    public class HealthCheckResult : IHealthCheckResult
    {
        public HealthCheckResultStatus Status { get; set; } = HealthCheckResultStatus.Unhealthy;
        public HealthCheckResultStatus DatabaseStatus { get; set; } =
            HealthCheckResultStatus.Unhealthy;
        public HealthCheckResultStatus CacheStatus { get; set; } =
            HealthCheckResultStatus.Unhealthy;
        public DateTime ServerTime { get; set; } = DateTime.UtcNow;
    }
}
