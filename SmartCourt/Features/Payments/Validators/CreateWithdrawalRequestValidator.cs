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
            .MaximumLength(200)
            .WithMessage("مرجع وجهة السحب القديم يجب ألا يتجاوز 200 حرفًا عند إرساله.");
    }

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(
            amount,
            2,
            MidpointRounding.AwayFromZero) == amount;
}
