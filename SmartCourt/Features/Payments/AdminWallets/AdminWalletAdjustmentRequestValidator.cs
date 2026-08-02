using FluentValidation;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments.Validators;

public sealed class AdminWalletAdjustmentRequestValidator
    : AbstractValidator<AdminWalletAdjustmentRequest>
{
    private const decimal MaximumAbsoluteDelta = 1_000_000m;

    public AdminWalletAdjustmentRequestValidator()
    {
        RuleFor(request => request.ContractId)
            .NotEmpty()
            .WithMessage("معرّف العقد المرتبط بالتصحيح المالي مطلوب.");
        RuleFor(request => request)
            .Must(request => request.PendingBalanceDelta != 0m
                || request.AvailableBalanceDelta != 0m)
            .WithMessage("يجب إدخال قيمة تصحيح غير صفرية لرصيد واحد على الأقل.");
        RuleFor(request => request.PendingBalanceDelta)
            .InclusiveBetween(-MaximumAbsoluteDelta, MaximumAbsoluteDelta)
            .WithMessage("قيمة تصحيح الرصيد المعلّق تتجاوز الحد الإداري المسموح.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("قيمة تصحيح الرصيد المعلّق يجب ألا تتجاوز منزلتين عشريتين.");
        RuleFor(request => request.AvailableBalanceDelta)
            .InclusiveBetween(-MaximumAbsoluteDelta, MaximumAbsoluteDelta)
            .WithMessage("قيمة تصحيح الرصيد المتاح تتجاوز الحد الإداري المسموح.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("قيمة تصحيح الرصيد المتاح يجب ألا تتجاوز منزلتين عشريتين.");
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب التصحيح المالي مطلوب.")
            .Must(reason => !string.IsNullOrWhiteSpace(reason))
            .WithMessage("سبب التصحيح المالي لا يمكن أن يكون فارغًا.")
            .MinimumLength(20)
            .WithMessage("سبب التصحيح المالي يجب أن يوضح المبرر في 20 حرفًا على الأقل.")
            .MaximumLength(1_500)
            .WithMessage("سبب التصحيح المالي يجب ألا يتجاوز 1500 حرف.");
    }

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero) == amount;
}
