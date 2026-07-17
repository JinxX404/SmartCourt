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

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("رقم الهاتف مطلوب.")
            .Matches("^\\+20\\d{10}$").WithMessage("رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX.");

        RuleFor(x => x.Address)
            .NotEmpty().WithMessage("العنوان مطلوب.")
            .MaximumLength(500).WithMessage("العنوان يجب أن لا يزيد عن 500 حرفًا.");

        RuleFor(x => x.Government)
            .NotEmpty().WithMessage("المحافظة مطلوبة.")
            .MaximumLength(100).WithMessage("المحافظة يجب أن لا تزيد عن 100 حرف.");

        RuleFor(x => x.City)
            .NotEmpty().WithMessage("المدينة مطلوبة.")
            .MaximumLength(100).WithMessage("المدينة يجب أن لا يزيد عن 100 حرف.");

        RuleFor(x => x.Gender)
            .NotEmpty().WithMessage("النوع مطلوب.")
            .MaximumLength(20).WithMessage("النوع يجب أن لا يزيد عن 20 حرفًا.");

        RuleFor(x => x.NationalNumber)
            .NotEmpty().WithMessage("الرقم القومي مطلوب.")
            .Length(14).WithMessage("الرقم القومي يجب أن يكون 14 رقمًا.")
            .Matches("^[0-9]{14}$").WithMessage("الرقم القومي غير صالح.");
    }
}
