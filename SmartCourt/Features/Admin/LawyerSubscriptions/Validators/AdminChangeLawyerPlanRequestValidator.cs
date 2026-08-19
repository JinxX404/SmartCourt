using FluentValidation;
using SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

namespace SmartCourt.Features.Admin.LawyerSubscriptions.Validators;

public sealed class AdminChangeLawyerPlanRequestValidator : AbstractValidator<AdminChangeLawyerPlanRequest>
{
    public AdminChangeLawyerPlanRequestValidator()
    {
        RuleFor(x => x.PlanType)
            .NotEmpty().WithMessage("يرجى تحديد الخطة.")
            .Must(x => x == "Free" || x == "Professional" || x == "Business").WithMessage("نوع الخطة غير صحيح.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("يرجى إدخال سبب التعديل.")
            .MaximumLength(500).WithMessage("سبب التعديل طويل جداً.");
    }
}
