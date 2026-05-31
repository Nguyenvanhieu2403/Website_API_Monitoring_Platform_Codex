using AutoMapper;
using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Features.NotificationChannels.Queries;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Queries;

public class GetNotificationChannelByIdQueryHandler : IRequestHandler<GetNotificationChannelByIdQuery, NotificationChannelDto>
{
    private readonly INotificationChannelRepository _channelRepository;
    private readonly IMapper _mapper;

    public GetNotificationChannelByIdQueryHandler(INotificationChannelRepository channelRepository, IMapper mapper)
    {
        _channelRepository = channelRepository;
        _mapper = mapper;
    }

    public async Task<NotificationChannelDto> Handle(GetNotificationChannelByIdQuery request, CancellationToken cancellationToken)
    {
        var channel = await _channelRepository.GetByIdAsync(request.ChannelId, request.OrganizationId);

        if (channel == null)
        {
            throw new KeyNotFoundException($"Notification Channel with ID {request.ChannelId} not found.");
        }

        return _mapper.Map<NotificationChannelDto>(channel);
    }
}
