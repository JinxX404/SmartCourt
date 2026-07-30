using SmartCourt.Features.Auth.RegisterLawyer.DTOs;
using FluentValidation;

namespace SmartCourt.Features.Auth.RegisterLawyer.Validators;

public class RegisterLawyerRequestValidator : AbstractValidator<RegisterLawyerRequest>
{
    public RegisterLawyerRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("الاسم الكامل مطلوب.")
            .MinimumLength(5).WithMessage("الاسم الكامل يجب أن لا يقل عن 5 أحرف.")
            .MaximumLength(150).WithMessage("الاسم الكامل يجب أن لا يزيد عن 150 حرفًا.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("البريد الإلكتروني مطلوب.")
            .EmailAddress().WithMessage("البريد الإلكتروني غير صالح.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("كلمة المرور مطلوبة.")
            .MinimumLength(8).WithMessage("كلمة المرور يجب أن تكون 8 أحرف على الأقل.")
            .Matches("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d).+$").WithMessage("كلمة المرور يجب أن تحتوي على حرف كبير وحرف صغير ورقم.");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("تأكيد كلمة المرور غير مطابق.");
    }
}
