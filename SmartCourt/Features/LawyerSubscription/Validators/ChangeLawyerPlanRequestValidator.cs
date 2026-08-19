using FluentValidation;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Enums;
using System;

namespace SmartCourt.Features.LawyerSubscription.Validators;

public sealed class ChangeLawyerPlanRequestValidator : AbstractValidator<ChangeLawyerPlanRequest>
{
    public ChangeLawyerPlanRequestValidator()
    {
        RuleFor(x => x.PlanType)
            .NotEmpty().WithMessage("نوع الخطة مطلوب.")
            .Must(BeAValidPlan).WithMessage("نوع الخطة غير صحيح.");
    }

    private bool BeAValidPlan(string planType)
    {
        return Enum.TryParse<LawyerPlanType>(planType, true, out _);
    }
}
