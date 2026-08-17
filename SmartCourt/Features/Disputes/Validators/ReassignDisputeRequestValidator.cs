using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class ReassignDisputeRequestValidator
    : AbstractValidator<ReassignDisputeRequest>
{
    public ReassignDisputeRequestValidator()
    {
        RuleFor(request => request.ModeratorUserId)
            .NotEmpty()
            .WithMessage("معرّف المشرف الجديد مطلوب.");

        RuleFor(request => request.Reason)
            .MaximumLength(2000)
            .When(request => !string.IsNullOrEmpty(request.Reason))
            .WithMessage("سبب إعادة التعيين يجب ألا يتجاوز 2000 حرف.");
    }
}
