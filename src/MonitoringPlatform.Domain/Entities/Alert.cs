namespace MonitoringPlatform.Domain.Entities;

public class Alert
{
    public Guid AlertId { get; set; }
    public Guid MonitorId { get; set; }
    public string Type { get; set; } = string.Empty; // "down", "recovery", "slow"
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; }

    // Navigation properties
    public virtual Monitor Monitor { get; set; } = null!;
}