using MediatR;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public record DeleteAlertRuleCommand(Guid RuleId, Guid OrganizationId) : IRequest<Unit>;
