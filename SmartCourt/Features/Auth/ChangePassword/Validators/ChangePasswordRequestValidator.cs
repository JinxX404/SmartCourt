using SmartCourt.Features.Auth.ChangePassword.DTOs;
using FluentValidation;
using SmartCourt.Extensions;

namespace SmartCourt.Features.Auth.ChangePassword.Validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("كلمة المرور الحالية مطلوبة");

        RuleFor(x => x.NewPassword)
            .Password();

        RuleFor(x => x.ConfirmNewPassword)
            .Equal(x => x.NewPassword).WithMessage("كلمة المرور وتأكيد كلمة المرور غير متطابقتين");
    }
}
