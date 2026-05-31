using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Queries;

public class GetAlertRulesListQueryHandler : IRequestHandler<GetAlertRulesListQuery, IEnumerable<AlertRuleDto>>
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public GetAlertRulesListQueryHandler(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<IEnumerable<AlertRuleDto>> Handle(GetAlertRulesListQuery request, CancellationToken cancellationToken)
    {
        var rules = await _alertRuleRepository.GetByMonitorIdAsync(request.MonitorId, request.OrganizationId);

        return rules.Select(rule => new AlertRuleDto
        {
            RuleId = rule.RuleId,
            OrganizationId = rule.OrganizationId,
            MonitorId = rule.MonitorId,
            Name = rule.Name,
            ConditionType = rule.ConditionType,
            ThresholdValue = rule.ThresholdValue,
            Severity = rule.Severity,
            IsEnabled = rule.IsEnabled,
            CooldownMinutes = rule.CooldownMinutes,
            CreatedAt = rule.CreatedAt,
            UpdatedAt = rule.UpdatedAt,
            NotificationChannels = rule.NotificationChannels.Select(nc => new NotificationChannelDto
            {
                ChannelId = nc.ChannelId,
                Name = nc.Name,
                Type = nc.Type,
                Configuration = nc.Configuration,
                IsEnabled = nc.IsEnabled
            }).ToList()
        }).ToList();
    }
}
