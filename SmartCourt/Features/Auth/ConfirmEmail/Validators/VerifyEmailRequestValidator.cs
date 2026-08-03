using FluentValidation;
using SmartCourt.Features.Auth.ConfirmEmail.DTOs;

namespace SmartCourt.Features.Auth.ConfirmEmail.Validators;

public class VerifyEmailRequestValidator : AbstractValidator<VerifyEmailRequest>
{
    private const int MaximumUserIdLength = 64;
    private const int MaximumEncodedTokenLength = 2048;

    public VerifyEmailRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("معرف المستخدم مطلوب.")
            .MaximumLength(MaximumUserIdLength).WithMessage("معرف المستخدم غير صالح.")
            .Must(id => Guid.TryParse(id, out _)).WithMessage("معرف المستخدم غير صالح.");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("رمز التأكيد مطلوب.")
            .MaximumLength(MaximumEncodedTokenLength).WithMessage("رمز التأكيد غير صالح.");
    }
}
