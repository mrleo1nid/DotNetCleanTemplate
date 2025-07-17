using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotNetCleanTemplate.Domain.Models
{
    public interface IHealthCheckResult
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
}
