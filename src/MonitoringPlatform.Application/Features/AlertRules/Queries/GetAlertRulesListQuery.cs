using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.AlertRules.Queries;

public record GetAlertRulesListQuery(Guid MonitorId, Guid OrganizationId) : IRequest<Result<IEnumerable<AlertRuleDto>>>;
