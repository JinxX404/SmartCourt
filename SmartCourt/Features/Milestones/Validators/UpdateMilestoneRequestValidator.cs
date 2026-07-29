using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class UpdateMilestoneRequestValidator
    : AbstractValidator<UpdateMilestoneRequest>
{
    public UpdateMilestoneRequestValidator(TimeProvider? timeProvider = null)
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
}
