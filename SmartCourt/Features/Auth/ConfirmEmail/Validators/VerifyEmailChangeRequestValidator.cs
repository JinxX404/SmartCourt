using FluentValidation;
using SmartCourt.Features.Auth.ConfirmEmail.DTOs;

namespace SmartCourt.Features.Auth.ConfirmEmail.Validators;

public class VerifyEmailChangeRequestValidator : AbstractValidator<VerifyEmailChangeRequest>
{
    public VerifyEmailChangeRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("معرف المستخدم مطلوب.");
        RuleFor(x => x.NewEmail).NotEmpty().EmailAddress().WithMessage("البريد الإلكتروني الجديد غير صالح.");
        RuleFor(x => x.Token).NotEmpty().WithMessage("رمز التأكيد مطلوب.");
    }
}
