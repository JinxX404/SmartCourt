using FluentValidation;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments.Validators;

public sealed class CreateMilestonePaymentSessionRequestValidator
    : AbstractValidator<CreateMilestonePaymentSessionRequest>
{
    public CreateMilestonePaymentSessionRequestValidator()
    {
        RuleFor(request => request.ConfirmationTokenReference)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^ctoken_[A-Za-z0-9_]+$")
            .WithMessage("A valid provider ConfirmationToken reference is required.");
    }
}
