using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public record GetNotificationChannelsListQuery(Guid OrganizationId) : IRequest<Result<IEnumerable<NotificationChannelDto>>>;
