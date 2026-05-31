using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IAlertEventRepository
{
    Task<AlertEvent?> GetByIdAsync(Guid eventId, Guid organizationId);
    Task<IEnumerable<AlertEvent>> GetByMonitorIdAsync(Guid monitorId, Guid organizationId);
    Task<AlertEvent> CreateAsync(AlertEvent alertEvent);
    Task UpdateAsync(AlertEvent alertEvent);
    Task<IEnumerable<AlertEvent>> GetPendingNotificationsAsync();
}
