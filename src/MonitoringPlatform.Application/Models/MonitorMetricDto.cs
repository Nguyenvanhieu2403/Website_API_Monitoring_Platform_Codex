namespace MonitoringPlatform.Application.Models;

public class MonitorMetricDto
{
    public Guid MonitorId { get; set; }
    public DateTime Timestamp { get; set; }
    public string Status { get; set; } = string.Empty; // "Up" or "Down"
    public int ResponseTime { get; set; }
    public string Unit { get; set; } = "ms";
}
