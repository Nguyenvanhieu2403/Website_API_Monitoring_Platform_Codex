using System;
using System.Collections.Generic;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Domain.Entities;

public class AlertRule
{
    public Guid RuleId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MonitorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AlertConditionType ConditionType { get; set; }
    public string? ThresholdValue { get; set; } // e.g., "500" for 500ms, "3" for 3 failures
    public AlertSeverity Severity { get; set; } = AlertSeverity.Critical;
    public bool IsEnabled { get; set; } = true;
    public int CooldownMinutes { get; set; } = 5; // Prevent alert spam
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public virtual Organization Organization { get; set; } = null!;
    public virtual Monitor Monitor { get; set; } = null!;
    public virtual ICollection<NotificationChannel> NotificationChannels { get; set; } = new List<NotificationChannel>();
}
