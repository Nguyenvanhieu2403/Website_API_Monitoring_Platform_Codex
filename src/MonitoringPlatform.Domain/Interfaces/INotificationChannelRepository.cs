using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface INotificationChannelRepository
{
    Task<NotificationChannel?> GetByIdAsync(Guid channelId, Guid organizationId);
    Task<IEnumerable<NotificationChannel>> GetByOrganizationIdAsync(Guid organizationId);
    Task<NotificationChannel> CreateAsync(NotificationChannel channel);
    Task UpdateAsync(NotificationChannel channel);
    Task DeleteAsync(Guid channelId, Guid organizationId);
    Task<IEnumerable<NotificationChannel>> GetChannelsForAlertRuleAsync(Guid alertRuleId);
}
