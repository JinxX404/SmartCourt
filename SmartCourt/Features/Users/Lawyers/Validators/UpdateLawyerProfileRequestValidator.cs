using FluentValidation;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Lawyers.DTOs;
using System;

namespace SmartCourt.Features.Users.Lawyers.Validators;

public class UpdateLawyerProfileRequestValidator : AbstractValidator<UpdateLawyerProfileRequest>
{
    public UpdateLawyerProfileRequestValidator()
    {

         RuleFor(x => x.NationalNumber)
         .NotEmpty()
         .Matches(@"^\d{14}$")
         .WithMessage("الرقم القومي يجب أن يتكون من 14 رقم بالضبط.");

         When(x => x.DateOfBirth.HasValue, () =>
         {
             RuleFor(x => x.DateOfBirth!.Value)
                 .LessThanOrEqualTo(DateOnly.FromDateTime(DateTime.Today).AddYears(-21))
                 .WithMessage("يجب أن يكون عمر المستخدم 21 عاماً أو أكثر.");
         });

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

         When(x => x.Specializations != null, () =>
         {
             RuleForEach(x => x.Specializations!).ChildRules(spec =>
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

             RuleFor(x => x.Specializations)
                 .Must(specs => specs!.Select(s => s.Specialization).Distinct().Count() == specs!.Count)
                 .WithMessage("لا يمكن تكرار نفس التخصص للمحامي.");
         });
    }
}
