using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class CreateMonitorCommandHandler : IRequestHandler<CreateMonitorCommand, MonitorDto>
{
    private readonly IMonitorRepository _monitorRepository;

    public CreateMonitorCommandHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<MonitorDto> Handle(CreateMonitorCommand request, CancellationToken cancellationToken)
    {
        var monitor = new Domain.Entities.Monitor
        {
            MonitorId = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Description = request.Description,
            Type = request.Type,
            Target = request.Target,
            Port = request.Port,
            IntervalSeconds = request.IntervalSeconds,
            TimeoutSeconds = request.TimeoutSeconds,
            Retries = request.Retries,
            FollowRedirects = request.FollowRedirects,
            ExpectedStatusCode = request.ExpectedStatusCode,
            ExpectedKeyword = request.ExpectedKeyword,
            HttpMethod = request.HttpMethod,
            CreatedAt = DateTime.UtcNow
        };

        // Note: For actual category/tag assignments, they are usually fetched and linked.
        // We'll write this into the repository method or handle it inside the repo implementation.
        // Let's create the monitor.
        var createdMonitor = await _monitorRepository.CreateAsync(monitor);

        // Fetch again to ensure category/tag info is included if linked (though on create it might be empty or mapped from request)
        return MapToDto(createdMonitor);
    }

    private static MonitorDto MapToDto(Domain.Entities.Monitor monitor)
    {
        return new MonitorDto
        {
            MonitorId = monitor.MonitorId,
            OrganizationId = monitor.OrganizationId,
            Name = monitor.Name,
            Description = monitor.Description,
            Type = monitor.Type,
            Target = monitor.Target,
            Port = monitor.Port,
            IntervalSeconds = monitor.IntervalSeconds,
            TimeoutSeconds = monitor.TimeoutSeconds,
            Retries = monitor.Retries,
            FollowRedirects = monitor.FollowRedirects,
            ExpectedStatusCode = monitor.ExpectedStatusCode,
            ExpectedKeyword = monitor.ExpectedKeyword,
            HttpMethod = monitor.HttpMethod,
            Status = monitor.Status,
            LastCheckedAt = monitor.LastCheckedAt,
            LastDownAt = monitor.LastDownAt,
            ResponseTimeMs = monitor.ResponseTimeMs,
            IsUp = monitor.IsUp,
            ConsecutiveFailures = monitor.ConsecutiveFailures,
            UptimePercentage = monitor.UptimePercentage,
            CreatedAt = monitor.CreatedAt,
            UpdatedAt = monitor.UpdatedAt,
            Categories = monitor.MonitorCategories?.Select(c => new MonitorCategoryDto
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Color = c.Color
            }).ToList() ?? new(),
            Tags = monitor.MonitorTags?.Select(t => new MonitorTagDto
            {
                TagId = t.TagId,
                Name = t.Name,
                Color = t.Color
            }).ToList() ?? new()
        };
    }
}
