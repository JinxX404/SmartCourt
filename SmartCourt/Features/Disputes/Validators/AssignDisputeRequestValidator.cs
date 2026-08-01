using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class AssignDisputeRequestValidator
    : AbstractValidator<AssignDisputeRequest>
{
    public AssignDisputeRequestValidator()
    {
        RuleFor(request => request.ModeratorUserId)
            .NotEmpty()
            .WithMessage("معرّف المشرف المطلوب تعيينه للنزاع مطلوب.");
    }
}
