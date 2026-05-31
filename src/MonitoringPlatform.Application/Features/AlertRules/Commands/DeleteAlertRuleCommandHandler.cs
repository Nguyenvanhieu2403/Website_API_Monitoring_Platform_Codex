using MediatR;
using MonitoringPlatform.Application.Models;
using MonitoringPlatform.Domain.Interfaces;

namespace MonitoringPlatform.Application.Features.AlertRules.Commands;

public class DeleteAlertRuleCommandHandler : IRequestHandler<DeleteAlertRuleCommand, Result>
{
    private readonly IAlertRuleRepository _alertRuleRepository;

    public DeleteAlertRuleCommandHandler(IAlertRuleRepository alertRuleRepository)
    {
        _alertRuleRepository = alertRuleRepository;
    }

    public async Task<Result> Handle(DeleteAlertRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = await _alertRuleRepository.GetByIdAsync(request.RuleId, request.OrganizationId);
        if (rule == null)
        {
            return Result.Failure("Không tìm thấy quy tắc cảnh báo.");
        }

        await _alertRuleRepository.DeleteAsync(request.RuleId, request.OrganizationId);
        return Result.Success();
    }
}
