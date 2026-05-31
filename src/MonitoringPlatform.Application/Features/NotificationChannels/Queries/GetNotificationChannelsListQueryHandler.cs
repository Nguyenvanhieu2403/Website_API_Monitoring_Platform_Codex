using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Features.NotificationChannels.Queries;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public class GetNotificationChannelsListQueryHandler : IRequestHandler<GetNotificationChannelsListQuery, IEnumerable<NotificationChannelDto>>
{
    private readonly INotificationChannelRepository _channelRepository;

    public GetNotificationChannelsListQueryHandler(INotificationChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<IEnumerable<NotificationChannelDto>> Handle(GetNotificationChannelsListQuery request, CancellationToken cancellationToken)
    {
        var channels = await _channelRepository.GetByOrganizationIdAsync(request.OrganizationId);

        return channels.Select(channel => new NotificationChannelDto
        {
            ChannelId = channel.ChannelId,
            OrganizationId = channel.OrganizationId,
            Name = channel.Name,
            Type = channel.Type,
            Configuration = channel.Configuration,
            IsEnabled = channel.IsEnabled,
            CreatedAt = channel.CreatedAt,
            UpdatedAt = channel.UpdatedAt
        }).ToList();
    }
}
