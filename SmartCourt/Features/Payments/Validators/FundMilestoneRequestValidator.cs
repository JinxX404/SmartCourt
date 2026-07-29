using FluentValidation;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments.Validators;

public sealed class FundMilestoneRequestValidator
    : AbstractValidator<FundMilestoneRequest>
{
    public FundMilestoneRequestValidator()
    {
        RuleFor(request => request.PaymentMethodReference)
            .NotEmpty()
            .WithMessage("مرجع وسيلة الدفع مطلوب.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("مرجع وسيلة الدفع لا يمكن أن يكون فارغًا.")
            .MaximumLength(200)
            .WithMessage("مرجع وسيلة الدفع يجب ألا يتجاوز 200 حرف.");
    }
}
