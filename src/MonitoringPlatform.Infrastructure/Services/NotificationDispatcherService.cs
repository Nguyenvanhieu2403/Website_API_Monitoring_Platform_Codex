using System.Text.Json;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace MonitoringPlatform.Infrastructure.Services;

public class NotificationDispatcherService : INotificationDispatcherService
{
    private readonly IEmailService _emailService;
    private readonly IWebhookService _webhookService;
    private readonly IAlertEventRepository _alertEventRepository;
    private readonly ILogger<NotificationDispatcherService> _logger;

    public NotificationDispatcherService(
        IEmailService emailService,
        IWebhookService webhookService,
        IAlertEventRepository alertEventRepository,
        ILogger<NotificationDispatcherService> logger)
    {
        _emailService = emailService;
        _webhookService = webhookService;
        _alertEventRepository = alertEventRepository;
        _logger = logger;
    }

    public async Task DispatchNotificationAsync(AlertEvent alertEvent, CancellationToken cancellationToken)
    {
        // Ensure AlertRule and NotificationChannels are loaded
        if (alertEvent.AlertRule == null || !alertEvent.AlertRule.NotificationChannels.Any())
        {
            _logger.LogWarning("Alert event {EventId} has no associated alert rule or notification channels. Skipping dispatch.", alertEvent.EventId);
            return;
        }

        foreach (var channel in alertEvent.AlertRule.NotificationChannels)
        {
            try
            {
                switch (channel.Type)
                {
                    case NotificationChannelType.Email:
                        await SendEmailNotification(alertEvent, channel);
                        break;
                    case NotificationChannelType.Webhook:
                        await SendWebhookNotification(alertEvent, channel);
                        break;
                    default:
                        _logger.LogWarning("Unknown notification channel type {ChannelType} for channel {ChannelId}. Skipping.", channel.Type, channel.ChannelId);
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error dispatching notification for AlertEvent {EventId} via channel {ChannelId}.", alertEvent.EventId, channel.ChannelId);
                // The exception is re-thrown by EmailService/WebhookService
                // This will be caught by the background worker for retry logic
                throw;
            }
        }

        // Mark as sent after successful dispatch to all channels
        alertEvent.IsNotificationSent = true;
        alertEvent.LastAttemptedAt = DateTime.UtcNow;
        await _alertEventRepository.UpdateAsync(alertEvent);
    }

    private async Task SendEmailNotification(AlertEvent alertEvent, NotificationChannel channel)
    {
        var config = JsonSerializer.Deserialize<EmailNotificationConfig>(channel.Configuration);
        if (config == null || string.IsNullOrWhiteSpace(config.RecipientEmail))
        {
            _logger.LogError("Email configuration missing or invalid for channel {ChannelId}", channel.ChannelId);
            return;
        }

        var subject = $"[{alertEvent.Severity}] Alert: {alertEvent.Message}";
        var body = $"<p><b>Monitor:</b> {alertEvent.Monitor?.Name} ({alertEvent.Monitor?.Target})</p>"
                   + $"<p><b>Alert Rule:</b> {alertEvent.AlertRule?.Name}</p>"
                   + $"<p><b>Condition:</b> {alertEvent.ConditionType}</p>"
                   + $"<p><b>Severity:</b> {alertEvent.Severity}</p>"
                   + $"<p><b>Message:</b> {alertEvent.Message}</p>"
                   + $"<p><b>Triggered At:</b> {alertEvent.TriggeredAt:yyyy-MM-dd HH:mm:ss UTC}</p>";

        await _emailService.SendEmailAsync(config.RecipientEmail, subject, body);
        _logger.LogInformation("Email notification sent for AlertEvent {EventId} to {RecipientEmail}", alertEvent.EventId, config.RecipientEmail);
    }

    private async Task SendWebhookNotification(AlertEvent alertEvent, NotificationChannel channel)
    {
        var config = JsonSerializer.Deserialize<WebhookNotificationConfig>(channel.Configuration);
        if (config == null || string.IsNullOrWhiteSpace(config.WebhookUrl))
        {
            _logger.LogError("Webhook configuration missing or invalid for channel {ChannelId}", channel.ChannelId);
            return;
        }

        var payload = new
        {
            alertEvent.EventId,
            alertEvent.OrganizationId,
            alertEvent.MonitorId,
            MonitorName = alertEvent.Monitor?.Name,
            MonitorTarget = alertEvent.Monitor?.Target,
            alertEvent.AlertRuleId,
            AlertRuleName = alertEvent.AlertRule?.Name,
            alertEvent.Severity,
            alertEvent.ConditionType,
            alertEvent.Message,
            alertEvent.TriggeredAt,
            alertEvent.IsResolved
        };

        await _webhookService.SendWebhookAsync(config.WebhookUrl, payload);
        _logger.LogInformation("Webhook notification sent for AlertEvent {EventId} to {WebhookUrl}", alertEvent.EventId, config.WebhookUrl);
    }
}

// Configuration DTOs for JSON serialization
public class EmailNotificationConfig
{
    public string RecipientEmail { get; set; } = string.Empty;
}

public class WebhookNotificationConfig
{
    public string WebhookUrl { get; set; } = string.Empty;
}
