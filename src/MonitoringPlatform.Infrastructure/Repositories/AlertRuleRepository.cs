using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class AlertRuleRepository : IAlertRuleRepository
{
    private readonly ApplicationDbContext _context;

    public AlertRuleRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlertRule?> GetByIdAsync(Guid ruleId, Guid organizationId)
    {
        return await _context.AlertRules
            .Include(ar => ar.NotificationChannels)
            .Include(ar => ar.Monitor)
            .FirstOrDefaultAsync(ar => ar.RuleId == ruleId && ar.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<AlertRule>> GetByMonitorIdAsync(Guid monitorId, Guid organizationId)
    {
        return await _context.AlertRules
            .Include(ar => ar.NotificationChannels)
            .Where(ar => ar.MonitorId == monitorId && ar.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<AlertRule> CreateAsync(AlertRule rule)
    {
        // Attach existing notification channels to avoid creating duplicates if IDs are provided
        if (rule.NotificationChannels.Any())
        {
            var channelIds = rule.NotificationChannels.Select(nc => nc.ChannelId).ToList();
            var existingChannels = await _context.NotificationChannels
                .Where(nc => channelIds.Contains(nc.ChannelId) && nc.OrganizationId == rule.OrganizationId)
                .ToListAsync();

            rule.NotificationChannels = existingChannels;
        }

        _context.AlertRules.Add(rule);
        await _context.SaveChangesAsync();
        return rule;
    }

    public async Task UpdateAsync(AlertRule rule)
    {
        var existingRule = await _context.AlertRules
            .Include(ar => ar.NotificationChannels)
            .FirstOrDefaultAsync(ar => ar.RuleId == rule.RuleId && ar.OrganizationId == rule.OrganizationId);

        if (existingRule != null)
        {
            _context.Entry(existingRule).CurrentValues.SetValues(rule);

            // Update Many-to-Many Relationship with NotificationChannels
            existingRule.NotificationChannels.Clear();
            if (rule.NotificationChannels.Any())
            {
                var channelIds = rule.NotificationChannels.Select(nc => nc.ChannelId).ToList();
                var existingChannels = await _context.NotificationChannels
                    .Where(nc => channelIds.Contains(nc.ChannelId) && nc.OrganizationId == rule.OrganizationId)
                    .ToListAsync();

                foreach (var channel in existingChannels)
                {
                    existingRule.NotificationChannels.Add(channel);
                }
            }

            await _context.SaveChangesAsync();
        }
    }

    public async Task DeleteAsync(Guid ruleId, Guid organizationId)
    {
        var rule = await GetByIdAsync(ruleId, organizationId);
        if (rule != null)
        {
            _context.AlertRules.Remove(rule);
            await _context.SaveChangesAsync();
        }
    }
}
