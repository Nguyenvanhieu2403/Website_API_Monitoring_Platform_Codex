using FluentValidation;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class UpdateMonitorCommandValidator : AbstractValidator<UpdateMonitorCommand>
{
    public UpdateMonitorCommandValidator()
    {
        RuleFor(x => x.MonitorId)
            .NotEmpty().WithMessage("ID Monitor là bắt buộc.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Tên là bắt buộc.")
            .MaximumLength(100).WithMessage("Tên không được vượt quá 100 ký tự.");

        RuleFor(x => x.Target)
            .NotEmpty().WithMessage("Mục tiêu (URL/IP) là bắt buộc.");

        RuleFor(x => x.IntervalSeconds)
            .InclusiveBetween(10, 3600).WithMessage("Khoảng thời gian kiểm tra phải từ 10 đến 3600 giây.");

        RuleFor(x => x.TimeoutSeconds)
            .InclusiveBetween(1, 60).WithMessage("Thời gian chờ phải từ 1 đến 60 giây.");
    }
}
