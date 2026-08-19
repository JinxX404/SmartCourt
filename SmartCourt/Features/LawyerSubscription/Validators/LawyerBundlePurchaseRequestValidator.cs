using FluentValidation;
using SmartCourt.Features.LawyerSubscription.DTOs;

namespace SmartCourt.Features.LawyerSubscription.Validators;

public sealed class LawyerBundlePurchaseRequestValidator : AbstractValidator<LawyerBundlePurchaseRequest>
{
    public LawyerBundlePurchaseRequestValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty().WithMessage("معرف الباقة مطلوب.");
    }
}
