using MediatR;
using MonitoringPlatform.Application.Features.NotificationChannels.Models;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Entities;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.NotificationChannels.Commands;

public class CreateNotificationChannelCommandHandler : IRequestHandler<CreateNotificationChannelCommand, Result<NotificationChannelDto>>
{
    private readonly INotificationChannelRepository _channelRepository;

    public CreateNotificationChannelCommandHandler(INotificationChannelRepository channelRepository)
    {
        _channelRepository = channelRepository;
    }

    public async Task<Result<NotificationChannelDto>> Handle(CreateNotificationChannelCommand request, CancellationToken cancellationToken)
    {
        var channel = new NotificationChannel
        {
            ChannelId = Guid.NewGuid(),
            OrganizationId = request.OrganizationId,
            Name = request.Name,
            Type = request.Type,
            Configuration = request.Configuration,
            IsEnabled = request.IsEnabled,
            CreatedAt = DateTime.UtcNow
        };

        var createdChannel = await _channelRepository.CreateAsync(channel);

        return Result<NotificationChannelDto>.Success(MapToDto(createdChannel));
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
