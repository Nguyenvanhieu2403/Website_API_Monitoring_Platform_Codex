using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.AlertEvents;

public record AlertLogDto
{
    public Guid EventId { get; init; }
    public Guid MonitorId { get; init; }
    public Guid AlertRuleId { get; init; }
    public string AlertRuleName { get; init; } = string.Empty;
    public AlertSeverity Severity { get; init; }
    public AlertConditionType ConditionType { get; init; }
    public string Message { get; init; } = string.Empty;
    public DateTime TriggeredAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public bool IsResolved { get; init; }
    public int AttemptCount { get; init; }
    public bool IsNotificationSent { get; init; }
}