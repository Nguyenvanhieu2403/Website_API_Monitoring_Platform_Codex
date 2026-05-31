using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public record UpdateAlertRuleCommand : IRequest<AlertRuleDto>
{
    public Guid RuleId { get; init; }
    public Guid OrganizationId { get; init; }
    public Guid MonitorId { get; init; } // Added MonitorId
    public string Name { get; init; } = string.Empty;
    public AlertConditionType ConditionType { get; init; }
    public string? ThresholdValue { get; init; }
    public AlertSeverity Severity { get; init; } = AlertSeverity.Critical;
    public bool IsEnabled { get; init; } = true;
    public int CooldownMinutes { get; init; } = 5;
    public List<Guid> ChannelIds { get; init; } = new();
}
