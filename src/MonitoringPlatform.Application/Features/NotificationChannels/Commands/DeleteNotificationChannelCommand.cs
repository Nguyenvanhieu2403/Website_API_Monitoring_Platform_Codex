using MediatR;
using MonitoringPlatform.Application.Models;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public record DeleteNotificationChannelCommand(Guid ChannelId, Guid OrganizationId) : IRequest<Result>;
