using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Interfaces;

public interface IMetricRepository
{
    Task<List<MonitorMetricDto>> GetMetricsForMonitorInTimeRangeAsync(Guid monitorId, string timeRange);
}
