using FluentValidation;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Lawyers.DTOs;
using System;

namespace SmartCourt.Features.Users.Lawyers.Validators;

public class CompleteLawyerProfileRequestValidator : AbstractValidator<CompleteLawyerProfileRequest>
{
    public CompleteLawyerProfileRequestValidator()
    {
         RuleFor(x => x.PhoneNumber)
         .NotEmpty()
         .Matches(@"^\+20\d{10}$")
         .WithMessage("رقم الهاتف يجب أن يكون بالتنسيق المصري +20XXXXXXXXXX");

         RuleFor(x => x.NationalNumber)
         .NotEmpty()
         .Length(14)
         .WithMessage("الرقم القومي يجب أن يتكون من 14 رقم.");

         RuleFor(x => x.Gender)
         .NotNull().WithMessage("الجنس مطلوب.")
         .IsInEnum().WithMessage("الجنس يجب أن يكون صالحاً.");

         RuleFor(x => x.DateOfBirth)
         .NotEmpty()
         .LessThan(DateOnly.FromDateTime(DateTime.Today))
         .WithMessage("يجب أن يكون تاريخ الميلاد في الماضي.");

         RuleFor(x => x.Level)
         .IsInEnum()
         .WithMessage("مستوى المحامي غير صالح.");

         RuleFor(x => x.Bio)
         .MaximumLength(500)
         .WithMessage("يجب ألا تتجاوز السيرة الذاتية 500 حرف.");

         RuleFor(x => x.Address)
         .MaximumLength(255)
         .WithMessage("يجب ألا يتجاوز العنوان 255 حرف.");
    }
}
