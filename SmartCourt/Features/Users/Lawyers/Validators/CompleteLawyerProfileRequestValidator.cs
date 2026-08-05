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

         RuleFor(x => x.Governorate)
         .MaximumLength(100)
         .WithMessage("يجب ألا تتجاوز المحافظة 100 حرف.");

         RuleFor(x => x.City)
         .MaximumLength(100)
         .WithMessage("يجب ألا تتجاوز المدينة 100 حرف.");

         RuleFor(x => x.Specializations)
         .NotEmpty()
         .WithMessage("يجب إدخال تخصص واحد على الأقل.");

         RuleForEach(x => x.Specializations).ChildRules(spec =>
         {
             spec.RuleFor(s => s.Specialization)
                 .IsInEnum()
                 .WithMessage("التخصص غير صالح.");

             spec.RuleFor(s => s.YearsOfExperience)
                 .GreaterThanOrEqualTo(0)
                 .WithMessage("سنوات الخبرة يجب أن تكون 0 أو أكثر.");

             spec.RuleFor(s => s.CasesHandled)
                 .GreaterThanOrEqualTo(0)
                 .WithMessage("عدد القضايا المنجزة يجب أن يكون 0 أو أكثر.");
         });
    }
}
