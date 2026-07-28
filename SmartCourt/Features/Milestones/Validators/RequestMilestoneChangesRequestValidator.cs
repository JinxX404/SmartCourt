using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class RequestMilestoneChangesRequestValidator
    : AbstractValidator<RequestMilestoneChangesRequest>
{
    public RequestMilestoneChangesRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب طلب التعديلات مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("سبب طلب التعديلات يجب ألا يتجاوز 2000 حرف.");
    }
}
