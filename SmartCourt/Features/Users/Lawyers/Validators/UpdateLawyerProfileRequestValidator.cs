using FluentValidation;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Users.Lawyers.DTOs;
using System;

namespace SmartCourt.Features.Users.Lawyers.Validators;

public class UpdateLawyerProfileRequestValidator : AbstractValidator<UpdateLawyerProfileRequest>
{
    public UpdateLawyerProfileRequestValidator()
    {
         RuleFor(x => x.PhoneNumber)
         .NotEmpty()
         .Matches(@"^(\+20|0)1[0125]\d{8}$")
         .WithMessage("رقم الهاتف يجب أن يكون رقم مصري صحيح (مثال: 011xxxxxxxx أو +2011xxxxxxxx)");

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
