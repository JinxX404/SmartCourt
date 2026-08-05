using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class RejectChangeRequestValidator
    : AbstractValidator<RejectChangeRequest>
{
    public RejectChangeRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب رفض طلب التعديل مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("سبب رفض طلب التعديل يجب ألا يتجاوز 2000 حرف.");
    }
}
