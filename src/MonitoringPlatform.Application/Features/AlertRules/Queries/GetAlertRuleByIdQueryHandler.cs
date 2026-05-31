using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Queries;

public class GetAlertRuleByIdQueryHandler : IRequestHandler<GetAlertRuleByIdQuery, AlertRuleDto>
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public GetAlertRuleByIdQueryHandler(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<AlertRuleDto> Handle(GetAlertRuleByIdQuery request, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(request.RuleId, request.OrganizationId);
        if (rule == null)
        {
            throw new KeyNotFoundException($"Alert Rule with ID {request.RuleId} not found.");
        }

        return MapToDto(rule);
    }

    private static AlertRuleDto MapToDto(Domain.Entities.AlertRule rule)
    {
        return new AlertRuleDto
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
        };
    }
}
