using System.Text.Json;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MonitoringPlatform.Application.Interfaces;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Infrastructure.BackgroundServices;

public class NotificationWorker : BackgroundService
{
    private readonly ILogger<NotificationWorker> _logger;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public NotificationWorker(
        ILogger<NotificationWorker> logger,
        IServiceScopeFactory serviceScopeFactory)
    {
        _logger = logger;
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Notification Worker running.");

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
                        if (alertEvent.AttemptCount >= 3)
                        {
                            _logger.LogWarning("Skipping AlertEvent {EventId} after {AttemptCount} failed attempts.", alertEvent.EventId, alertEvent.AttemptCount);
                            alertEvent.IsNotificationSent = false; // Ensure it's not marked as sent if all retries failed
                            alertEvent.LastAttemptedAt = DateTime.UtcNow;
                            await alertEventRepository.UpdateAsync(alertEvent);
                            continue;
                        }

                        try
                        {
                            _logger.LogInformation("Attempting to dispatch notification for AlertEvent {EventId}. Attempt {AttemptCount}", alertEvent.EventId, alertEvent.AttemptCount + 1);
                            await notificationDispatcherService.DispatchNotificationAsync(alertEvent, stoppingToken);
                            _logger.LogInformation("Successfully dispatched notification for AlertEvent {EventId}", alertEvent.EventId);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Failed to dispatch notification for AlertEvent {EventId}. Attempt {AttemptCount}", alertEvent.EventId, alertEvent.AttemptCount + 1);

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

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken); // Check for new alerts every 30 seconds
        }

        _logger.LogInformation("Notification Worker stopped.");
    }
}
