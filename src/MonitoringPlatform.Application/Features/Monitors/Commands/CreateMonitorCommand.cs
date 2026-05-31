using MediatR;
using MonitoringPlatform.Application.Features.Monitors.Models;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public record CreateMonitorCommand : IRequest<MonitorDto>
{
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public MonitorType Type { get; init; }
    public string Target { get; init; } = string.Empty;
    public int? Port { get; init; }
    public int IntervalSeconds { get; init; } = 60;
    public int TimeoutSeconds { get; init; } = 30;
    public int? Retries { get; init; } = 3;
    public bool FollowRedirects { get; init; } = true;
    public string? ExpectedStatusCode { get; init; }
    public string? ExpectedKeyword { get; init; }
    public string? HttpMethod { get; init; } = "GET";
    public List<Guid> CategoryIds { get; init; } = new();
    public List<Guid> TagIds { get; init; } = new();
}