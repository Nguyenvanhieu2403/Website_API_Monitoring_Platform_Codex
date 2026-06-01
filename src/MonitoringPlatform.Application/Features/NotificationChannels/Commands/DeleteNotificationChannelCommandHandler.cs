using MediatR;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public class DeleteNotificationChannelCommandHandler : IRequestHandler<DeleteNotificationChannelCommand, Result>
{
    private readonly INotificationChannelRepository _channelRepository;

    public DeleteNotificationChannelCommandHandler(INotificationChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result> Handle(DeleteNotificationChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, request.OrganizationId);
        if (channel == null)
        {
            return Result.Failure("Không tìm thấy kênh thông báo.");
        }

        await _channelRepository.DeleteAsync(request.ChannelId, request.OrganizationId);
        return Result.Success();
    }
}
