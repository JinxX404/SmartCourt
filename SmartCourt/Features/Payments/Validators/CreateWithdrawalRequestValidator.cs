using FluentValidation;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments.Validators;

public sealed class CreateWithdrawalRequestValidator
    : AbstractValidator<CreateWithdrawalRequest>
{
    public CreateWithdrawalRequestValidator()
    {
        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .WithMessage("قيمة السحب يجب أن تكون أكبر من صفر بالجنيه المصري.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("قيمة السحب يجب ألا تتجاوز منزلتين عشريتين.");
        RuleFor(request => request.DestinationReference)
            .NotEmpty()
            .WithMessage("مرجع وجهة السحب مطلوب.")
            .Must(value => !string.IsNullOrWhiteSpace(value))
            .WithMessage("مرجع وجهة السحب لا يمكن أن يكون فارغًا.")
            .MaximumLength(200)
            .WithMessage("مرجع وجهة السحب يجب ألا يتجاوز 200 حرف.");
    }

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero) == amount;
}
