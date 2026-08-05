using SmartCourt.Features.Auth.ResetPassword.DTOs;
using FluentValidation;
using SmartCourt.Extensions;

namespace SmartCourt.Features.Auth.ResetPassword.Validators;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("عنوان البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("عنوان البريد الإلكتروني غير صالح");

        RuleFor(x => x.NewPassword)
            .Password();

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("كلمة المرور وتأكيد كلمة المرور غير متطابقتين");
    }
}
