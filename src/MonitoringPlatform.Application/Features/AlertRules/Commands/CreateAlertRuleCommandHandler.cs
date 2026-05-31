using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public class CreateAlertRuleCommandHandler : IRequestHandler<CreateAlertRuleCommand, Result<AlertRuleDto>>
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly INotificationChannelRepository _notificationChannelRepository;

    public CreateAlertRuleCommandHandler(IAlertRuleRepository alertRuleRepository, INotificationChannelRepository notificationChannelRepository)
    {
        _alertRuleRepository = alertRuleRepository;
        _notificationChannelRepository = notificationChannelRepository;
    }

    public async Task<Result<AlertRuleDto>> Handle(CreateAlertRuleCommand request, CancellationToken cancellationToken)
    {
        // Validate monitor existence (though it should be validated by previous steps or FluentValidation)
        // For now, let's assume monitor exists as per Clean Architecture principles where application layer handles business rules.

        var channels = new List<NotificationChannel>();
        foreach (var id in request.ChannelIds)
        {
            var channel = await _notificationChannelRepository.GetByIdAsync(id, request.OrganizationId);
            if (channel != null)
            {
                channels.Add(channel);
            }
            else
            {
                return Result<AlertRuleDto>.Failure($"Kênh thông báo với ID {id} không tìm thấy.");
            }
        }

        var rule = new AlertRule
        {
            RuleId = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            MonitorId = request.MonitorId,
            Name = request.Name,
            ConditionType = request.ConditionType,
            ThresholdValue = request.ThresholdValue,
            Severity = request.Severity,
            IsEnabled = request.IsEnabled,
            CooldownMinutes = request.CooldownMinutes,
            NotificationChannels = channels,
            CreatedAt = DateTime.UtcNow
        };

        var createdRule = await _alertRuleRepository.CreateAsync(rule);

        return Result<AlertRuleDto>.Success(MapToDto(createdRule));
    }

    private static AlertRuleDto MapToDto(AlertRule rule)
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
