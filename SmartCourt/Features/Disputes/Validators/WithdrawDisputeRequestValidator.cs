using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class WithdrawDisputeRequestValidator
    : AbstractValidator<WithdrawDisputeRequest>
{
    public WithdrawDisputeRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب سحب النزاع مطلوب.")
            .MaximumLength(2000)
            .WithMessage("سبب سحب النزاع يجب ألا يتجاوز 2000 حرف.");
    }
}
