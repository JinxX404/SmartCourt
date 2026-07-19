using FluentValidation;
using SmartCourt.Features.Auth.ConfirmEmail.DTOs;

namespace SmartCourt.Features.Auth.ConfirmEmail.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("معرف المستخدم مطلوب.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("رمز التأكيد مطلوب.");
    }
}
