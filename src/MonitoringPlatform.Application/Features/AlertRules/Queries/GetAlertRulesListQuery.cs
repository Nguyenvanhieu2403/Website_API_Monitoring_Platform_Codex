using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;

namespace MonitoringPlatform.Application.Features.AlertRules.Queries;

public record GetAlertRulesListQuery(Guid MonitorId, Guid OrganizationId) : IRequest<IEnumerable<AlertRuleDto>>;
