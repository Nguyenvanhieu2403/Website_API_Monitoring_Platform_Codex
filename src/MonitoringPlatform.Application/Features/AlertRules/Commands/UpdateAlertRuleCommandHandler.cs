using MediatR;
using MonitoringPlatform.Application.Features.AlertRules.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public class UpdateAlertRuleCommandHandler : IRequestHandler<UpdateAlertRuleCommand, Result<AlertRuleDto>>
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly INotificationChannelRepository _notificationChannelRepository;

    public UpdateAlertRuleCommandHandler(IAlertRuleRepository alertRuleRepository, INotificationChannelRepository notificationChannelRepository)
    {
        _alertRuleRepository = alertRuleRepository;
        _notificationChannelRepository = notificationChannelRepository;
    }

    public async Task<Result<AlertRuleDto>> Handle(UpdateAlertRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(request.RuleId, request.OrganizationId);
        if (rule == null)
        {
            return Result<AlertRuleDto>.Failure("Không tìm thấy quy tắc cảnh báo.");
        }

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

        rule.Name = request.Name;
        rule.ConditionType = request.ConditionType;
        rule.ThresholdValue = request.ThresholdValue;
        rule.Severity = request.Severity;
        rule.IsEnabled = request.IsEnabled;
        rule.CooldownMinutes = request.CooldownMinutes;
        rule.NotificationChannels = channels;
        rule.UpdatedAt = DateTime.UtcNow;

        await _alertRuleRepository.UpdateAsync(rule);

        return Result<AlertRuleDto>.Success(MapToDto(rule));
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
