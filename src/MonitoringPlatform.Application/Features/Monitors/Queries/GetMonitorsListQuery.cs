using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Enums;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Queries;

public record GetMonitorsListQuery : IRequest<Result<PagedResponse<MonitorDto>>>
{
    public Guid OrganizationId { get; init; }
    public string? Search { get; init; }
    public MonitorType? Type { get; init; }
    public MonitorStatus? Status { get; init; }
    public Guid? CategoryId { get; init; }
    public Guid? TagId { get; init; }
    public bool? IsUp { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
    public string? SortBy { get; init; } = "CreatedAt";
    public bool SortDescending { get; init; } = true;

    public MonitorFilter ToFilter() => new MonitorFilter
    {
        Search = Search,
        Type = Type,
        Status = Status,
        CategoryId = CategoryId,
        TagId = TagId,
        IsUp = IsUp,
        PageNumber = PageNumber,
        PageSize = PageSize,
        SortBy = SortBy,
        SortDescending = SortDescending
    };
}
