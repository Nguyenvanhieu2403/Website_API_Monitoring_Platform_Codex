using MediatR;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public record DeleteMonitorCommand : IRequest<Result>
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
}
