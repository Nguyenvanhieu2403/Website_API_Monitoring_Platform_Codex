using MediatR;
using MonitoringPlatform.Application.Common.Events;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertEvents.EventHandlers;

public class MonitorCheckedEventHandler : INotificationHandler<MonitorCheckedEvent>
{
    private readonly IAlertRuleRepository _alertRuleRepository;
    private readonly IAlertEventRepository _alertEventRepository;
    private readonly IMonitorRepository _monitorRepository;

    public MonitorCheckedEventHandler(
        IAlertRuleRepository alertRuleRepository,
        IAlertEventRepository alertEventRepository,
        IMonitorRepository monitorRepository)
    {
        _alertRuleRepository = alertRuleRepository;
        _alertEventRepository = alertEventRepository;
        _monitorRepository = monitorRepository;
    }

    public async Task Handle(MonitorCheckedEvent notification, CancellationToken cancellationToken)
    {
        var monitor = await _monitorRepository.GetByIdAsync(notification.MonitorId, notification.OrganizationId);
        if (monitor == null) return;

        var alertRules = await _alertRuleRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId);

        foreach (var rule in alertRules)
        {
            var triggerAlert = false;
            var alertMessage = string.Empty;

            switch (rule.ConditionType)
            {
                case AlertConditionType.MonitorDown:
                    if (!notification.IsUp)
                    {
                        triggerAlert = true;
                        alertMessage = $"Monitor \'{monitor.Name}\' is down.";
                    }
                    break;
                case AlertConditionType.MonitorUp:
                    if (notification.IsUp)
                    {
                        triggerAlert = true;
                        alertMessage = $"Monitor \'{monitor.Name}\' is up (recovery).";
                    }
                    break;
                case AlertConditionType.ResponseTimeThreshold:
                    if (int.TryParse(rule.ThresholdValue, out var threshold) && notification.ResponseTimeMs > threshold)
                    {
                        triggerAlert = true;
                        alertMessage = $"Monitor \'{monitor.Name}\' response time ({notification.ResponseTimeMs}ms) exceeded threshold ({threshold}ms).";
                    }
                    break;
                case AlertConditionType.FailureCountThreshold:
                    if (int.TryParse(rule.ThresholdValue, out var failureCountThreshold))
                    {
                        var recentAlerts = (await _alertEventRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId))
                            .Where(ae => ae.AlertRuleId == rule.RuleId && ae.ConditionType == AlertConditionType.FailureCountThreshold && !ae.IsResolved && ae.TriggeredAt > DateTime.UtcNow.AddMinutes(-rule.CooldownMinutes))
                            .ToList();

                        if (!notification.IsUp && recentAlerts.Count + 1 >= failureCountThreshold)
                        {
                            triggerAlert = true;
                            alertMessage = $"Monitor \'{monitor.Name}\' has failed {recentAlerts.Count + 1} times, exceeding failure count threshold ({failureCountThreshold}).";
                        }
                    }
                    break;
                default:
                    // Unknown condition type, skip rule
                    continue;
            }

            if (triggerAlert)
            {
                // Check for alert spam prevention
                var lastAlertEvent = (await _alertEventRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId))
                    .FirstOrDefault(ae => ae.AlertRuleId == rule.RuleId && !ae.IsResolved);

                if (lastAlertEvent == null || lastAlertEvent.TriggeredAt.AddMinutes(rule.CooldownMinutes) < DateTime.UtcNow)
                {
                    var newAlertEvent = new AlertEvent
                    {
                        EventId = Guid.NewGuid(),
                        OrganizationId = notification.OrganizationId,
                        MonitorId = notification.MonitorId,
                        AlertRuleId = rule.RuleId,
                        Severity = rule.Severity,
                        ConditionType = rule.ConditionType,
                        Message = alertMessage,
                        TriggeredAt = notification.CheckedAt,
                        IsResolved = false
                    };
                    await _alertEventRepository.CreateAsync(newAlertEvent);
                }
            }
            else if (rule.ConditionType == AlertConditionType.MonitorUp)
            {
                // If the monitor is now up, resolve any active 'MonitorDown' alerts for this rule
                var activeMonitorDownAlert = (await _alertEventRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId))
                    .FirstOrDefault(ae => ae.AlertRuleId == rule.RuleId && ae.ConditionType == AlertConditionType.MonitorDown && !ae.IsResolved);

                if (activeMonitorDownAlert != null)
                {
                    activeMonitorDownAlert.IsResolved = true;
                    activeMonitorDownAlert.ResolvedAt = notification.CheckedAt;
                    await _alertEventRepository.UpdateAsync(activeMonitorDownAlert);
                }
            }

            // Resolve 'ResponseTimeThreshold' or 'FailureCountThreshold' alerts if conditions are no longer met
            if (rule.ConditionType == AlertConditionType.ResponseTimeThreshold && !triggerAlert)
            {
                var activeResponseTimeAlert = (await _alertEventRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId))
                    .FirstOrDefault(ae => ae.AlertRuleId == rule.RuleId && ae.ConditionType == AlertConditionType.ResponseTimeThreshold && !ae.IsResolved);

                if (activeResponseTimeAlert != null)
                {
                    activeResponseTimeAlert.IsResolved = true;
                    activeResponseTimeAlert.ResolvedAt = notification.CheckedAt;
                    await _alertEventRepository.UpdateAsync(activeResponseTimeAlert);
                }
            }

            if (rule.ConditionType == AlertConditionType.FailureCountThreshold && !triggerAlert)
            {
                var activeFailureCountAlert = (await _alertEventRepository.GetByMonitorIdAsync(notification.MonitorId, notification.OrganizationId))
                    .FirstOrDefault(ae => ae.AlertRuleId == rule.RuleId && ae.ConditionType == AlertConditionType.FailureCountThreshold && !ae.IsResolved);

                if (activeFailureCountAlert != null)
                {
                    activeFailureCountAlert.IsResolved = true;
                    activeFailureCountAlert.ResolvedAt = notification.CheckedAt;
                    await _alertEventRepository.UpdateAsync(activeFailureCountAlert);
                }
            }
        }
    }
}
