using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.Monitors.Queries;

public record GetMonitorByIdQuery : IRequest<Result<MonitorDto>>
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
}
