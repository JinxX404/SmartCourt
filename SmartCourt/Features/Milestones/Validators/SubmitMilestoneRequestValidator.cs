using FluentValidation;
using SmartCourt.Features.Milestones.DTOs;

namespace SmartCourt.Features.Milestones.Validators;

public sealed class SubmitMilestoneRequestValidator
    : AbstractValidator<SubmitMilestoneRequest>
{
    public SubmitMilestoneRequestValidator()
    {
        RuleFor(request => request.Notes)
            .NotEmpty()
            .WithMessage("ملاحظات التسليم مطلوبة.")
            .MaximumLength(10_000)
            .WithMessage("ملاحظات التسليم يجب ألا تتجاوز 10000 حرف.");
        RuleFor(request => request.StoredFileIds)
            .NotNull()
            .WithMessage("يجب إرفاق ملف واحد على الأقل.")
            .Must(HasAuthorizedFileIds)
            .WithMessage("يجب تحديد معرّفات ملفات صالحة ومصرح بها.");
    }

    private static bool HasAuthorizedFileIds(
        IReadOnlyList<Guid>? storedFileIds)
    {
        return storedFileIds is { Count: > 0 }
            && storedFileIds.All(id => id != Guid.Empty)
            && storedFileIds.Distinct().Count() == storedFileIds.Count;
    }
}
