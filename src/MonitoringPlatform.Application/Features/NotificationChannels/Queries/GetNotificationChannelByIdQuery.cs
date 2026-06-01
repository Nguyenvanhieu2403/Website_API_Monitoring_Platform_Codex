using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public record GetNotificationChannelByIdQuery(Guid ChannelId, Guid OrganizationId) : IRequest<Result<NotificationChannelDto>>;
