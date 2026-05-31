using Microsoft.EntityFrameworkCore;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;
using MonitoringPlatform.Infrastructure.Data;

namespace MonitoringPlatform.Infrastructure.Repositories;

public class AlertEventRepository : IAlertEventRepository
{
    private readonly ApplicationDbContext _context;

    public AlertEventRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AlertEvent?> GetByIdAsync(Guid eventId, Guid organizationId)
    {
        return await _context.AlertEvents
            .Include(ae => ae.Monitor)
            .Include(ae => ae.AlertRule)
            .FirstOrDefaultAsync(ae => ae.EventId == eventId && ae.OrganizationId == organizationId);
    }

    public async Task<IEnumerable<AlertEvent>> GetByMonitorIdAsync(Guid monitorId, Guid organizationId)
    {
        return await _context.AlertEvents
            .Where(ae => ae.MonitorId == monitorId && ae.OrganizationId == organizationId)
            .OrderByDescending(ae => ae.TriggeredAt)
            .ToListAsync();
    }

    public async Task<AlertEvent> CreateAsync(AlertEvent alertEvent)
    {
        _context.AlertEvents.Add(alertEvent);
        await _context.SaveChangesAsync();
        return alertEvent;
    }

    public async Task UpdateAsync(AlertEvent alertEvent)
    {
        _context.AlertEvents.Update(alertEvent);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<AlertEvent>> GetPendingNotificationsAsync()
    {
        return await _context.AlertEvents
            .Include(ae => ae.Monitor)
            .Include(ae => ae.AlertRule)
            .ThenInclude(ar => ar.NotificationChannels)
            .Where(ae => !ae.IsNotificationSent && ae.AttemptCount < 3)
            .ToListAsync();
    }
}
