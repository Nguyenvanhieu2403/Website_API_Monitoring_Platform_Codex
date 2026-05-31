using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public record GetNotificationChannelsListQuery(Guid OrganizationId) : IRequest<IEnumerable<NotificationChannelDto>>;
