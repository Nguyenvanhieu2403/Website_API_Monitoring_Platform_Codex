using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public record UpdateMonitorCommand : IRequest<Result<MonitorDto>>
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public MonitorType Type { get; init; }
    public string Target { get; init; } = string.Empty;
    public int? Port { get; init; }
    public int IntervalSeconds { get; init; }
    public int TimeoutSeconds { get; init; }
    public int? Retries { get; init; }
    public bool FollowRedirects { get; init; }
    public string? ExpectedStatusCode { get; init; }
    public string? ExpectedKeyword { get; init; }
    public string? HttpMethod { get; init; }
    public List<Guid> CategoryIds { get; init; } = new();
    public List<Guid> TagIds { get; init; } = new();
}
