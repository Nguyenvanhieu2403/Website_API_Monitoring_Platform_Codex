using MediatR;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class DeleteMonitorCommandHandler : IRequestHandler<DeleteMonitorCommand, Unit>
{
    private readonly IMonitorRepository _monitorRepository;

    public DeleteMonitorCommandHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<Unit> Handle(DeleteMonitorCommand request, CancellationToken cancellationToken)
    {
        var monitor = await _monitorRepository.GetByIdAsync(request.MonitorId, request.OrganizationId);
        if (monitor == null)
        {
            throw new KeyNotFoundException($"Monitor with ID {request.MonitorId} was not found.");
        }

        await _monitorRepository.DeleteAsync(request.MonitorId, request.OrganizationId);
        return Unit.Value;
    }
}
