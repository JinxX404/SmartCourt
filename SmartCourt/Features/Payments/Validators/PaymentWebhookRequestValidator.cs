using FluentValidation;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Validators;

public sealed class PaymentWebhookRequestValidator
    : AbstractValidator<PaymentWebhookRequest>
{
    public PaymentWebhookRequestValidator()
    {
        RuleFor(request => request.EventId)
            .NotEmpty()
            .WithMessage("معرّف حدث الدفع مطلوب.")
            .MaximumLength(200)
            .WithMessage("معرّف حدث الدفع يجب ألا يتجاوز 200 حرف.");
        RuleFor(request => request.PaymentTransactionId)
            .NotEmpty()
            .WithMessage("معرّف معاملة الدفع مطلوب.");
        RuleFor(request => request.ProviderTransactionId)
            .NotEmpty()
            .WithMessage("معرّف معاملة مزود الدفع مطلوب.")
            .MaximumLength(200)
            .WithMessage(
                "معرّف معاملة مزود الدفع يجب ألا يتجاوز 200 حرف.");
        RuleFor(request => request.Status)
            .IsInEnum()
            .WithMessage("حالة معاملة الدفع غير صالحة.")
            .Must(status => status is
                PaymentTransactionStatus.Processing
                or PaymentTransactionStatus.Completed
                or PaymentTransactionStatus.Failed)
            .WithMessage("حالة معاملة الدفع غير مدعومة.");
        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .WithMessage("قيمة معاملة الدفع يجب أن تكون أكبر من صفر.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage(
                "قيمة معاملة الدفع يجب ألا تتجاوز منزلتين عشريتين.");
        RuleFor(request => request.Currency)
            .Equal("EGP")
            .WithMessage("عملة الدفع يجب أن تكون الجنيه المصري EGP.");
        RuleFor(request => request.ProcessedAt)
            .Must(value => !value.HasValue
                || value.Value.Offset == TimeSpan.Zero)
            .WithMessage(
                "تاريخ معالجة الدفع يجب أن يكون بالتوقيت العالمي.");
        RuleFor(request => request.FailureReason)
            .MaximumLength(2_000)
            .WithMessage("سبب فشل الدفع يجب ألا يتجاوز 2000 حرف.");
    }

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero) == amount;
}
