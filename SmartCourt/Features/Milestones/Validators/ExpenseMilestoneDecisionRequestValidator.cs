using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class ExpenseMilestoneDecisionRequestValidator
    : AbstractValidator<ExpenseMilestoneDecisionRequest>
{
    public ExpenseMilestoneDecisionRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب القرار مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("سبب القرار يجب ألا يتجاوز 2000 حرف.");
    }
}
