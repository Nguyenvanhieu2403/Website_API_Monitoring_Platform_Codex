using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public class GetNotificationChannelByIdQueryHandler : IRequestHandler<GetNotificationChannelByIdQuery, Result<NotificationChannelDto>>
{
    private readonly INotificationChannelRepository _channelRepository;

    public GetNotificationChannelByIdQueryHandler(INotificationChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result<NotificationChannelDto>> Handle(GetNotificationChannelByIdQuery request, CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, request.OrganizationId);
        if (channel == null)
        {
            return Result<NotificationChannelDto>.Failure("Không tìm thấy kênh thông báo.");
        }

        return Result<NotificationChannelDto>.Success(MapToDto(channel));
    }

    private static NotificationChannelDto MapToDto(NotificationChannel channel)
    {
        return new NotificationChannelDto
        {
            ChannelId = channel.ChannelId,
            OrganizationId = channel.OrganizationId,
            Name = channel.Name,
            Type = channel.Type,
            Configuration = channel.Configuration,
            IsEnabled = channel.IsEnabled,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt
        };
    }
}
