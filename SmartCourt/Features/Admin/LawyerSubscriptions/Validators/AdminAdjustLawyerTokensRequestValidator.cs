using FluentValidation;
using SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;

namespace SmartCourt.Features.Admin.LawyerSubscriptions.Validators;

public sealed class AdminAdjustLawyerTokensRequestValidator : AbstractValidator<AdminAdjustLawyerTokensRequest>
{
    public AdminAdjustLawyerTokensRequestValidator()
    {
        RuleFor(x => x.CreditAmount)
            .NotEqual(0).WithMessage("يجب أن لا تكون القيمة صفر.");

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("يرجى إدخال سبب التعديل.")
            .MaximumLength(500).WithMessage("سبب التعديل طويل جداً.");
    }
}
