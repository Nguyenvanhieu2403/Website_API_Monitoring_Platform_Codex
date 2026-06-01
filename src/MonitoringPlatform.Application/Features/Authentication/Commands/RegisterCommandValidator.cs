using FluentValidation;

namespace MonitoringPlatform.Application.Features.Authentication.Commands;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email là bắt buộc.")
            .EmailAddress().WithMessage("Định dạng email không hợp lệ.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Mật khẩu là bắt buộc.")
            .MinimumLength(6).WithMessage("Mật khẩu phải có ít nhất 6 ký tự.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Họ là bắt buộc.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Tên là bắt buộc.");
    }
}
