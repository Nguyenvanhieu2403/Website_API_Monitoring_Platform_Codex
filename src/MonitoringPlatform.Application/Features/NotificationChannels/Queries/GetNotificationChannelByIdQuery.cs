using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public record GetNotificationChannelByIdQuery(Guid ChannelId, Guid OrganizationId) : IRequest<NotificationChannelDto>;
