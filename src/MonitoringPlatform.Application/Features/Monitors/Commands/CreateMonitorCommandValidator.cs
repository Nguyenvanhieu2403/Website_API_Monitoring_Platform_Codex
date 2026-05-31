using FluentValidation;
using MonitoringPlatform.Domain.Enums;

namespace MonitoringPlatform.Application.Features.Monitors.Commands;

public class CreateMonitorCommandValidator : AbstractValidator<CreateMonitorCommand>
{
    public CreateMonitorCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid monitor type.");

        RuleFor(x => x.Target)
            .NotEmpty().WithMessage("Target is required.")
            .MaximumLength(2048).WithMessage("Target cannot exceed 2048 characters.");

        RuleFor(x => x.IntervalSeconds)
            .GreaterThanOrEqualTo(10).WithMessage("Interval must be at least 10 seconds.");

        RuleFor(x => x.TimeoutSeconds)
            .GreaterThan(0).WithMessage("Timeout must be greater than 0.")
            .LessThanOrEqualTo(x => x.IntervalSeconds).WithMessage("Timeout cannot be greater than the interval.");

        RuleFor(x => x.Retries)
            .GreaterThanOrEqualTo(0).WithMessage("Retries must be greater than or equal to 0.");

        RuleFor(x => x.HttpMethod)
            .MaximumLength(10).WithMessage("HTTP Method cannot exceed 10 characters.")
            .Must(method => string.IsNullOrEmpty(method) ||
                            method == "GET" || method == "POST" || method == "PUT" ||
                            method == "DELETE" || method == "PATCH" || method == "HEAD" || method == "OPTIONS")
            .WithMessage("Invalid HTTP Method.");
    }
}
