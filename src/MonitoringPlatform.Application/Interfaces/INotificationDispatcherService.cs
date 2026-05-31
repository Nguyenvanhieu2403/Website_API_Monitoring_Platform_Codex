using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Application.Interfaces;

public interface INotificationDispatcherService
{
    Task DispatchNotificationAsync(AlertEvent alertEvent, CancellationToken cancellationToken);
}
