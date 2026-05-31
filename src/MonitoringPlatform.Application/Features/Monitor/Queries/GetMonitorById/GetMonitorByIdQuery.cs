using MediatR;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Application.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitor.Queries.GetMonitorById;

public record GetMonitorByIdQuery(Guid MonitorId, Guid OrganizationId) : IRequest<Result<MonitorDto>>;

public class GetMonitorByIdQueryHandler : IRequestHandler<GetMonitorByIdQuery, Result<MonitorDto>>
{
    private readonly IMonitorRepository _monitorRepository;

    public GetMonitorByIdQueryHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<Result<MonitorDto>> Handle(GetMonitorByIdQuery request, CancellationToken cancellationToken)
    {
        var monitor = await _monitorRepository.GetByIdAsync(request.MonitorId, request.OrganizationId);

        if (monitor == null)
        {
            return Result<MonitorDto>.Failure($"Monitor with ID {request.MonitorId} not found.");
        }

        var monitorDto = new MonitorDto
        {
            MonitorId = monitor.MonitorId,
            OrganizationId = monitor.OrganizationId,
            Name = monitor.Name,
            Target = monitor.Target,
            Type = monitor.Type,
            Interval = monitor.IntervalSeconds,
            Timeout = monitor.TimeoutSeconds,
            Status = monitor.Status,
            IsUp = monitor.IsUp,
            Description = monitor.Description,
            CreatedAt = monitor.CreatedAt,
            UpdatedAt = monitor.UpdatedAt,
            LastCheckedAt = monitor.LastCheckedAt,
            Headers = monitor.RequestHeaders,
            HttpMethod = monitor.HttpMethod,
            ExpectedStatusCode = monitor.ExpectedStatusCode,
            ExpectedContent = monitor.ExpectedKeyword,
            MonitorCategories = monitor.MonitorCategories.Select(mc => new MonitorCategoryDto
            {
                CategoryId = mc.CategoryId,
                Name = mc.Name
            }).ToList(),
            MonitorTags = monitor.MonitorTags.Select(mt => new MonitorTagDto
            {
                TagId = mt.TagId,
                Name = mt.Name
            }).ToList()
        };

        return Result<MonitorDto>.Success(monitorDto);
    }
}
