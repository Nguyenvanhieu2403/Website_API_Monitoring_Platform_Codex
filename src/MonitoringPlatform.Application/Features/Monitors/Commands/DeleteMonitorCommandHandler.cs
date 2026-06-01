using MediatR;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class DeleteMonitorCommandHandler : IRequestHandler<DeleteMonitorCommand, Result>
{
    private readonly IMonitorRepository _monitorRepository;

    public DeleteMonitorCommandHandler(IMonitorRepository monitorRepository)
    {
        _monitorRepository = monitorRepository;
    }

    public async Task<Result> Handle(DeleteMonitorCommand request, CancellationToken cancellationToken)
    {
        var monitor = await _monitorRepository.GetByIdAsync(request.MonitorId, request.OrganizationId);
        if (monitor == null)
        {
            return Result.Failure("Không tìm thấy monitor.");
        }

        await _monitorRepository.DeleteAsync(request.MonitorId, request.OrganizationId);
        return Result.Success();
    }
}
