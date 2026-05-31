using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class NotificationChannelRepository : INotificationChannelRepository
{
    private readonly ApplicationDbContext _context;

    public NotificationChannelRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<NotificationChannel?> GetByIdAsync(Guid channelId, Guid organizationId)
    {
        return await _context.NotificationChannels
            .FirstOrDefaultAsync(nc => nc.ChannelId == channelId && nc.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<NotificationChannel>> GetByOrganizationIdAsync(Guid organizationId)
    {
        return await _context.NotificationChannels
            .Where(nc => nc.OrganizationId == organizationId)
            .ToListAsync();
    }

    public async Task<NotificationChannel> CreateAsync(NotificationChannel channel)
    {
        _context.NotificationChannels.Add(channel);
        await _context.SaveChangesAsync();
        return channel;
    }

    public async Task UpdateAsync(NotificationChannel channel)
    {
        _context.NotificationChannels.Update(channel);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid channelId, Guid organizationId)
    {
        var channel = await GetByIdAsync(channelId, organizationId);
        if (channel != null)
        {
            _context.NotificationChannels.Remove(channel);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<IEnumerable<NotificationChannel>> GetChannelsForAlertRuleAsync(Guid alertRuleId)
    {
        return await _context.AlertRules
            .Where(ar => ar.RuleId == alertRuleId)
            .SelectMany(ar => ar.NotificationChannels)
            .ToListAsync();
    }
}
