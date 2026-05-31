using MediatR;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public class DeleteAlertRuleCommandHandler : IRequestHandler<DeleteAlertRuleCommand, Unit>
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public DeleteAlertRuleCommandHandler(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<Unit> Handle(DeleteAlertRuleCommand request, CancellationToken cancellationToken)
    {
        await _alertRuleRepository.DeleteAsync(request.RuleId, request.OrganizationId);
        return Unit.Value;
    }
}
