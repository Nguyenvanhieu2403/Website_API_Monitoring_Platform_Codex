namespace MonitoringPlatform.Domain.Entities;

public class MonitorLog
{
    public Guid LogId { get; set; }
    public Guid MonitorId { get; set; }
    public bool IsUp { get; set; }
    public int ResponseTimeMs { get; set; }
    public int? StatusCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string? ResponseBody { get; set; }
    public DateTime CheckedAt { get; set; }

    // Navigation properties
    public virtual Monitor Monitor { get; set; } = null!;
}