using FluentValidation;

namespace SmartCourt.Features.Auth.ForgotPassword;

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("عنوان البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("عنوان البريد الإلكتروني غير صالح");
    }
}
