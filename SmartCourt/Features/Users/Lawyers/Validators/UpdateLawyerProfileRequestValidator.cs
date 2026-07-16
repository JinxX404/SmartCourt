using FluentValidation;
using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Validators;

public class UpdateLawyerProfileRequestValidator : AbstractValidator<UpdateLawyerProfileRequest>
{
    public UpdateLawyerProfileRequestValidator()
    {

         RuleFor(x => x.Email)
         .NotEmpty()
         .EmailAddress()
         .MaximumLength(255)
         .WithMessage("البريد الإلكتروني غير صالح.");

         RuleFor(x => x.PhoneNumber)
         .NotEmpty()
         .Matches(@"^\+?[0-9]{7,15}$")
         .WithMessage("رقم الهاتف غير صالح.");

         RuleFor(x => x.DateOfBirth)
         .NotEmpty()
         .WithMessage("يجب إدخال تاريخ الميلاد.");

         RuleFor(x => x.Specialization)
         .NotEmpty()
         .WithMessage("يجب إدخال التخصص.");

         RuleFor(x => x.YearsOfExperience)
         .NotEmpty()
         .WithMessage("يجب إدخال عدد سنوات الخبرة.");

        RuleFor(x => x.YearsOfExperience)
        .GreaterThanOrEqualTo(0)
        .WithMessage("عدد سنوات الخبرة يجب أن يكون صفر أو أكبر.");

         RuleFor(x => x.Bio)
         .MaximumLength(500)
         .WithMessage("يجب ألا تتجاوز السيرة الذاتية 500 حرف.");

         RuleFor(x => x.Address)
         .MaximumLength(255)
         .WithMessage("يجب ألا يتجاوز العنوان 255 حرف.");
    }
}
