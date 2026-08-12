using FluentValidation;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Idempotency;

namespace SmartCourt.Features.Payments.Validators;

public sealed class RetryPaymentSessionRequestValidator
    : AbstractValidator<RetryPaymentSessionRequest>
{
    public RetryPaymentSessionRequestValidator()
    {
        RuleFor(request => request.ConfirmationTokenReference)
            .NotEmpty()
            .MaximumLength(200)
            .Matches("^ctoken_[A-Za-z0-9_]+$")
            .WithMessage("A valid provider ConfirmationToken reference is required.");
        RuleFor(request => request.IdempotencyKey)
            .NotEmpty()
            .MaximumLength(IdempotencyHeader.MaximumLength);
    }
}
