using MonitoringPlatform.Domain.Entities;

namespace MonitoringPlatform.Domain.Interfaces;

public interface IAlertRuleRepository
{
    Task<AlertRule?> GetByIdAsync(Guid ruleId, Guid organizationId);
    Task<IEnumerable<AlertRule>> GetByMonitorIdAsync(Guid monitorId, Guid organizationId);
    Task<AlertRule> CreateAsync(AlertRule rule);
    Task UpdateAsync(AlertRule rule);
    Task DeleteAsync(Guid ruleId, Guid organizationId);
}
