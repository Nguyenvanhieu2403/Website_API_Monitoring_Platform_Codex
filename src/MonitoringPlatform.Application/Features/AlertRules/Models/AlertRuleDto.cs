using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.AlertRules.Models;

public class AlertRuleDto
{
    public Guid RuleId { get; set; }
    public Guid OrganizationId { get; set; }
    public Guid MonitorId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AlertConditionType ConditionType { get; set; }
    public string? ThresholdValue { get; set; }
    public AlertSeverity Severity { get; set; }
    public bool IsEnabled { get; set; }
    public int CooldownMinutes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<NotificationChannelDto> NotificationChannels { get; set; } = new();
}

public class NotificationChannelDto
{
    public Guid ChannelId { get; set; }
    public string Name { get; set; } = string.Empty;
    public NotificationChannelType Type { get; set; }
    public string Configuration { get; set; } = string.Empty;
    public bool IsEnabled { get; set; }
}

public class AlertEventDto
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
    public int AttemptCount { get; set; }
    public DateTime? LastAttemptedAt { get; set; }
    public bool IsNotificationSent { get; set; }
}
