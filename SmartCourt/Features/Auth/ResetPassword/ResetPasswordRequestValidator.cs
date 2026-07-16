using FluentValidation;

namespace SmartCourt.Features.Auth.ResetPassword;

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("عنوان البريد الإلكتروني مطلوب")
            .EmailAddress().WithMessage("عنوان البريد الإلكتروني غير صالح");

        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("رمز إعادة التعيين مطلوب");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("كلمة المرور الجديدة مطلوبة")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل");

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("كلمة المرور وتأكيد كلمة المرور غير متطابقتين");
    }
}
