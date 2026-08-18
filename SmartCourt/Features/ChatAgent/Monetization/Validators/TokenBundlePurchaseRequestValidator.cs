using FluentValidation;
using SmartCourt.Features.ChatAgent.Monetization.DTOs;

namespace SmartCourt.Features.ChatAgent.Monetization.Validators;

public class TokenBundlePurchaseRequestValidator : AbstractValidator<TokenBundlePurchaseRequest>
{
    public TokenBundlePurchaseRequestValidator()
    {
        RuleFor(x => x.BundleId)
            .NotEmpty().WithMessage("معرف الباقة مطلوب.");
    }
}
