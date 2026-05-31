using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class UpdateMonitorCommandHandler : IRequestHandler<UpdateMonitorCommand, Result<MonitorDto>>
{
    private readonly IMonitorRepository _monitorRepository;

    public UpdateMonitorCommandHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<Result<MonitorDto>> Handle(UpdateMonitorCommand request, CancellationToken cancellationToken)
    {
        var monitor = await _monitorRepository.GetByIdAsync(request.MonitorId, request.OrganizationId);
        if (monitor == null)
        {
            return Result<MonitorDto>.Failure("Không tìm thấy monitor.");
        }

        monitor.Name = request.Name;
        monitor.Description = request.Description;
        monitor.Type = request.Type;
        monitor.Target = request.Target;
        monitor.Port = request.Port;
        monitor.IntervalSeconds = request.IntervalSeconds;
        monitor.TimeoutSeconds = request.TimeoutSeconds;
        monitor.Retries = request.Retries;
        monitor.FollowRedirects = request.FollowRedirects;
        monitor.ExpectedStatusCode = request.ExpectedStatusCode;
        monitor.ExpectedKeyword = request.ExpectedKeyword;
        monitor.HttpMethod = request.HttpMethod;
        monitor.UpdatedAt = DateTime.UtcNow;

        await _monitorRepository.UpdateAsync(monitor);

        return Result<MonitorDto>.Success(MapToDto(monitor));
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
