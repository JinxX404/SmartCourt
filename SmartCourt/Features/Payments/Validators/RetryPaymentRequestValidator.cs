using FluentValidation;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Idempotency;

namespace SmartCourt.Features.Payments.Validators;

public sealed class RetryPaymentRequestValidator
    : AbstractValidator<RetryPaymentRequest>
{
    public RetryPaymentRequestValidator()
    {
        RuleFor(request => request.IdempotencyKey)
            .NotEmpty()
            .WithMessage(
                "مفتاح Idempotency-Key مطلوب لإعادة محاولة عملية الدفع.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage(
                "مفتاح Idempotency-Key لا يمكن أن يكون فارغًا.")
            .MaximumLength(IdempotencyHeader.MaximumLength)
            .WithMessage(
                $"مفتاح Idempotency-Key يجب ألا يتجاوز {IdempotencyHeader.MaximumLength} حرف.");
    }
}
