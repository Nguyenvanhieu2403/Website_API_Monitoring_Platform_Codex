using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;

namespace MonitoringPlatform.Application.Features.Monitors.Queries;

public record GetMonitorByIdQuery : IRequest<MonitorDto>
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
}