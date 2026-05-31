using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class MetricRepository :
    MonitoringPlatform.Domain.Interfaces.IMetricRepository,
    MonitoringPlatform.Application.Interfaces.IMetricRepository
{
    private readonly ApplicationDbContext _context;

    public MetricRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // Domain implementation
    async Task<List<MonitorMetric>> MonitoringPlatform.Domain.Interfaces.IMetricRepository.GetMetricsForMonitorInTimeRangeAsync(Guid monitorId, string timeRange)
    {
        DateTime startTime = CalculateStartTime(timeRange);

        return await _context.MonitorLogs
            .Where(m => m.MonitorId == monitorId && m.CheckedAt >= startTime)
            .OrderBy(m => m.CheckedAt)
            .Select(m => new MonitorMetric
            {
                MonitorId = m.MonitorId,
                Timestamp = m.CheckedAt,
                ResponseTime = m.ResponseTimeMs,
                Status = m.IsUp ? "Up" : "Down"
            })
            .ToListAsync();
    }

    // Application implementation
    public async Task<List<MonitorMetricDto>> GetMetricsForMonitorInTimeRangeAsync(Guid monitorId, string timeRange)
    {
        DateTime startTime = CalculateStartTime(timeRange);

        return await _context.MonitorLogs
            .Where(m => m.MonitorId == monitorId && m.CheckedAt >= startTime)
            .OrderBy(m => m.CheckedAt)
            .Select(m => new MonitorMetricDto
            {
                MonitorId = m.MonitorId,
                Timestamp = m.CheckedAt,
                ResponseTime = m.ResponseTimeMs,
                Status = m.IsUp ? "Up" : "Down",
                Unit = "ms"
            })
            .ToListAsync();
    }

    private DateTime CalculateStartTime(string timeRange)
    {
        var now = DateTime.UtcNow;
        return timeRange.ToLower() switch
        {
            "1h" => now.AddHours(-1),
            "24h" => now.AddDays(-1),
            "7d" => now.AddDays(-7),
            "30d" => now.AddDays(-30),
            _ => throw new ArgumentException("Invalid time range specified.", nameof(timeRange))
        };
    }
}
