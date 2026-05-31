
using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IMetricRepository
{
    Task<List<MonitorMetric>> GetMetricsForMonitorInTimeRangeAsync(Guid monitorId, string timeRange);
}
