using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Entities;

public class AlertEvent
{
    public Guid EventId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MonitorId { get; set; }
    public Guid AlertRuleId { get; set; }
    public AlertSeverity Severity { get; set; }
    public AlertConditionType ConditionType { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime TriggeredAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public bool IsResolved { get; set; }
    public int AttemptCount { get; set; } = 0; // For notification retries
    public DateTime? LastAttemptedAt { get; set; }
    public bool IsNotificationSent { get; set; } = false;

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Monitor Monitor { get; set; } = null!;
    public virtual AlertRule AlertRule { get; set; } = null!;
}
