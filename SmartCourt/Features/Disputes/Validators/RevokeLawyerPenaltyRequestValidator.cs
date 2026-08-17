using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class RevokeLawyerPenaltyRequestValidator
    : AbstractValidator<RevokeLawyerPenaltyRequest>
{
    public RevokeLawyerPenaltyRequestValidator()
    {
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب إلغاء العقوبة مطلوب.")
            .MaximumLength(2000)
            .WithMessage("سبب إلغاء العقوبة يجب ألا يتجاوز 2000 حرف.");
    }
}
