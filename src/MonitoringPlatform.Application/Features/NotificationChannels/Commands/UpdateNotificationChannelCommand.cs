using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public record UpdateNotificationChannelCommand : IRequest<NotificationChannelDto>
{
    public Guid ChannelId { get; init; }
    public Guid OrganizationId { get; init; }
    public string Name { get; init; } = string.Empty;
    public NotificationChannelType Type { get; init; }
    public string Configuration { get; init; } = string.Empty;
    public bool IsEnabled { get; init; }
}
