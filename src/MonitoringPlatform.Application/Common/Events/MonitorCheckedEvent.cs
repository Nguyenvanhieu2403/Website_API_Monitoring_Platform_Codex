using MediatR;

namespace MonitoringPlatform.Application.Common.Events;

public record MonitorCheckedEvent : INotification
{
    public Guid MonitorId { get; init; }
    public Guid OrganizationId { get; init; }
    public bool IsUp { get; init; }
    public int ResponseTimeMs { get; init; }
    public int? StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CheckedAt { get; init; }
}
