using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class AddMilestoneRequestValidator
    : AbstractValidator<AddMilestoneRequest>
{
    public AddMilestoneRequestValidator(TimeProvider? timeProvider = null)
    {
        var clock = timeProvider ?? TimeProvider.System;

        RuleFor(request => request.Title)
            .NotEmpty()
            .WithMessage("عنوان المرحلة مطلوب.")
            .Length(3, 200)
            .WithMessage("عنوان المرحلة يجب أن يكون بين 3 و200 حرف.");
        RuleFor(request => request.Description)
            .Must(value =>
                value is null || !string.IsNullOrWhiteSpace(value))
            .WithMessage("وصف المرحلة لا يمكن أن يكون فارغًا.")
            .MaximumLength(10_000)
            .WithMessage("وصف المرحلة يجب ألا يتجاوز 10000 حرف.");
        RuleFor(request => request.OrderNumber)
            .GreaterThan(0)
            .WithMessage("ترتيب المرحلة يجب أن يكون أكبر من صفر.");
        RuleFor(request => request.Amount)
            .GreaterThan(0)
            .WithMessage("قيمة المرحلة يجب أن تكون أكبر من صفر بالجنيه المصري.")
            .Must(HasAtMostTwoDecimalPlaces)
            .WithMessage("قيمة المرحلة يجب ألا تتجاوز منزلتين عشريتين.");
        RuleFor(request => request.DurationDays)
            .InclusiveBetween(1, 365)
            .When(request => request.DurationDays.HasValue)
            .WithMessage("مدة المرحلة يجب أن تكون بين يوم واحد و365 يومًا.");
        RuleFor(request => request.DueDate)
            .Must(date =>
                !date.HasValue
                || date.Value > clock.GetUtcNow().UtcDateTime)
            .WithMessage("تاريخ استحقاق المرحلة يجب أن يكون في المستقبل.");
    }

    private static bool HasAtMostTwoDecimalPlaces(decimal amount)
        => decimal.Round(amount, 2, MidpointRounding.AwayFromZero) == amount;
}
