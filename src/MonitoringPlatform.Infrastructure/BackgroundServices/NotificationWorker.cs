using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Settings;

namespace MonitoringPlatform.Infrastructure.BackgroundServices;

public class NotificationWorker : BackgroundService
{
    private readonly NotificationWorkerSettings _settings;
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationWorker(
        IOptions<NotificationWorkerSettings> options,
        ILogger<NotificationWorker> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _settings = options.Value;
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation("Notification Worker is disabled.");
            return;
        }

        _logger.LogInformation("Notification Worker running with polling interval of {PollingInterval} seconds and max {MaxRetries} retry attempts.",
            _settings.PollingIntervalSeconds, _settings.MaxRetryAttempts);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var alertEventRepository = scope.ServiceProvider.GetRequiredService<IAlertEventRepository>();
                    var notificationDispatcherService = scope.ServiceProvider.GetRequiredService<INotificationDispatcherService>();

                    var pendingAlertEvents = await alertEventRepository.GetPendingNotificationsAsync();

                    foreach (var alertEvent in pendingAlertEvents)
                    {
                        if (alertEvent.AttemptCount >= _settings.MaxRetryAttempts)
                        {
                            _logger.LogWarning("Skipping AlertEvent {EventId} after {AttemptCount} failed attempts (max: {MaxRetries}).",
                                alertEvent.EventId, alertEvent.AttemptCount, _settings.MaxRetryAttempts);
                            alertEvent.IsNotificationSent = false; // Ensure it's not marked as sent if all retries failed
                            alertEvent.LastAttemptedAt = DateTime.UtcNow;
                            await alertEventRepository.UpdateAsync(alertEvent);
                            continue;
                        }

                        try
                        {
                            _logger.LogInformation("Attempting to dispatch notification for AlertEvent {EventId}. Attempt {AttemptCount}/{MaxRetries}",
                                alertEvent.EventId, alertEvent.AttemptCount + 1, _settings.MaxRetryAttempts);
                            await notificationDispatcherService.DispatchNotificationAsync(alertEvent, stoppingToken);
                            _logger.LogInformation("Successfully dispatched notification for AlertEvent {EventId}", alertEvent.EventId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to dispatch notification for AlertEvent {EventId}. Attempt {AttemptCount}/{MaxRetries}",
                                alertEvent.EventId, alertEvent.AttemptCount + 1, _settings.MaxRetryAttempts);

                            alertEvent.AttemptCount++;
                            alertEvent.LastAttemptedAt = DateTime.UtcNow;
                            alertEvent.IsNotificationSent = false; // Mark as not sent for retry
                            await alertEventRepository.UpdateAsync(alertEvent);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in Notification Worker.");
            }

            await Task.Delay(TimeSpan.FromSeconds(_settings.PollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Notification Worker stopped.");
    }
}
