using MediatR;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public class DeleteNotificationChannelCommandHandler : IRequestHandler<DeleteNotificationChannelCommand, Unit>
{
    private readonly INotificationChannelRepository _channelRepository;

    public DeleteNotificationChannelCommandHandler(INotificationChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Unit> Handle(DeleteNotificationChannelCommand request, CancellationToken cancellationToken)
    {
        await _channelRepository.DeleteAsync(request.ChannelId, request.OrganizationId);
        return Unit.Value;
    }
}
