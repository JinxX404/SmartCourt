using FluentValidation;

namespace SmartCourt.Features.Auth.ResendVerification;

public class ResendVerificationRequestValidator : AbstractValidator<ResendVerificationRequest>
{
    public ResendVerificationRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("عنوان البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("عنوان البريد الإلكتروني غير صالح");
    }
}
