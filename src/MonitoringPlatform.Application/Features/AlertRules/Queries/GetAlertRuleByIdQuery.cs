using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.AlertRules.Queries;

public record GetAlertRuleByIdQuery(Guid RuleId, Guid OrganizationId) : IRequest<Result<AlertRuleDto>>;
