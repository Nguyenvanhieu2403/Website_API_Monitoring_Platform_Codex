using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Models;

public record NotificationChannelDto
{
    public Guid ChannelId { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public NotificationChannelType Type { get; init; }
    public string Configuration { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
