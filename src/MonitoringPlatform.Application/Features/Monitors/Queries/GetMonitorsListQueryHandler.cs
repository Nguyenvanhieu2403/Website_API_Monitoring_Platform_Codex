using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Queries;

public class GetMonitorsListQueryHandler : IRequestHandler<GetMonitorsListQuery, PagedResponse<MonitorDto>>
{
    private readonly IMonitorRepository _monitorRepository;

    public GetMonitorsListQueryHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<PagedResponse<MonitorDto>> Handle(GetMonitorsListQuery request, CancellationToken cancellationToken)
    {
        var filter = request.ToFilter();
        var pagedResult = await _monitorRepository.GetPagedAsync(request.OrganizationId, filter);

        var dtos = pagedResult.Items.Select(MapToDto).ToList();

        return new PagedResponse<MonitorDto>
        {
            Items = dtos,
            TotalCount = pagedResult.TotalCount,
            PageNumber = pagedResult.PageNumber,
            PageSize = pagedResult.PageSize
        };
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
