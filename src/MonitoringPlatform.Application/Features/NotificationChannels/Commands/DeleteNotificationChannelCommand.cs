using MediatR;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public record DeleteNotificationChannelCommand(Guid ChannelId, Guid OrganizationId) : IRequest<Unit>;
