using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class CreateMilestoneChangeRequestValidator
    : AbstractValidator<CreateMilestoneChangeRequest>
{
    public CreateMilestoneChangeRequestValidator(
        TimeProvider? timeProvider = null)
    {
        var clock = timeProvider ?? TimeProvider.System;

        RuleFor(request => request.ProposedDescription)
            .MaximumLength(10_000)
            .WithMessage("الوصف المقترح يجب ألا يتجاوز 10000 حرف.")
            .Must(value =>
                value is null || !string.IsNullOrWhiteSpace(value))
            .WithMessage("الوصف المقترح لا يمكن أن يكون فارغًا.");
        RuleFor(request => request.ProposedDurationDays)
            .InclusiveBetween(1, 365)
            .When(request => request.ProposedDurationDays.HasValue)
            .WithMessage(
                "المدة المقترحة يجب أن تكون بين يوم واحد و365 يومًا.");
        RuleFor(request => request.ProposedDueDate)
            .Must(date =>
                !date.HasValue
                || date.Value > clock.GetUtcNow().UtcDateTime)
            .WithMessage("تاريخ الاستحقاق المقترح يجب أن يكون في المستقبل.");
        RuleFor(request => request)
            .Must(HasAtLeastOneChange)
            .WithMessage("يجب أن يتضمن طلب التعديل تغييرًا واحدًا على الأقل.");
        RuleFor(request => request.Reason)
            .NotEmpty()
            .WithMessage("سبب طلب التعديل مطلوب.")
            .MaximumLength(2_000)
            .WithMessage("سبب طلب التعديل يجب ألا يتجاوز 2000 حرف.");
    }

    private static bool HasAtLeastOneChange(
        CreateMilestoneChangeRequest request)
    {
        return !string.IsNullOrWhiteSpace(request.ProposedDescription)
            || request.ProposedDurationDays.HasValue
            || request.ProposedDueDate.HasValue;
    }
}
