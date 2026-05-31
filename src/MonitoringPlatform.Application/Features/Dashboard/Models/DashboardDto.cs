namespace MonitoringPlatform.Application.Models;

public class DashboardDto
{
    public Guid OrganizationId { get; set; }
    public List<MonitorOverviewDto> MonitorOverviews { get; set; } = new();
}

public class MonitorOverviewDto
{
    public Guid MonitorId { get; set; }
    public string MonitorName { get; set; } = string.Empty;
    public decimal UptimePercentage { get; set; }
    public int AverageResponseTime { get; set; }
    public int MinResponseTime { get; set; }
    public int MaxResponseTime { get; set; }
    public decimal FailureRate { get; set; }
    public List<MonitorStatusTimelineDto> StatusTimeline { get; set; } = new();
}

public class ResponseTimeMetricsDto
{
    public int Average { get; set; }
    public int Min { get; set; }
    public int Max { get; set; }
}

public class MonitorStatusTimelineDto
{
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty; // e.g., "Up", "Down"
}

public class AggregatedAnalyticsDto
{
    public Guid OrganizationId { get; set; }
    public int TotalMonitors { get; set; }
    public int TotalChecks { get; set; }
    public decimal AverageUptimePercentage { get; set; }
    public int AverageResponseTime { get; set; }
    public decimal AverageFailureRate { get; set; }
}
