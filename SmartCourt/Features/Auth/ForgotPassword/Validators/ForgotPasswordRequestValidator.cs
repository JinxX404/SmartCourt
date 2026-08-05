using SmartCourt.Features.Auth.ForgotPassword.DTOs;
using FluentValidation;

namespace SmartCourt.Features.Auth.ForgotPassword.Validators;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("عنوان البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("عنوان البريد الإلكتروني غير صالح");
    }
}
