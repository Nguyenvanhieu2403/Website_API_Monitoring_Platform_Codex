using MediatR;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public record DeleteAlertRuleCommand(Guid RuleId, Guid OrganizationId) : IRequest<Result>;
