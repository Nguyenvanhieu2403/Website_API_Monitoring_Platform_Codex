using MediatR;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public record DeleteMonitorCommand : IRequest<Unit>
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
}