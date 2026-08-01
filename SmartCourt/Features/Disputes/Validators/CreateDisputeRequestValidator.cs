using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class CreateDisputeRequestValidator
    : AbstractValidator<CreateDisputeRequest>
{
    public CreateDisputeRequestValidator()
    {
        RuleFor(request => request.MilestoneId)
            .NotEmpty()
            .WithMessage("معرّف المرحلة مطلوب لفتح النزاع.");
        RuleFor(request => request.Category)
            .IsInEnum()
            .WithMessage("تصنيف النزاع غير صالح.");
        RuleFor(request => request.Title)
            .NotEmpty()
            .WithMessage("عنوان النزاع مطلوب.")
            .Length(3, 200)
            .WithMessage("عنوان النزاع يجب أن يتراوح بين 3 و200 حرف.");
        RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("وصف النزاع مطلوب.")
            .MaximumLength(20_000)
            .WithMessage("وصف النزاع يجب ألا يتجاوز 20000 حرف.");
        RuleFor(request => request.RequestedOutcome)
            .IsInEnum()
            .WithMessage("النتيجة المطلوبة للنزاع غير صالحة.");
        RuleFor(request => request.StoredFileIds)
            .NotNull()
            .WithMessage("قائمة ملفات الأدلة مطلوبة.")
            .Must(ids => ids is not null
                && ids.All(id => id != Guid.Empty)
                && ids.Distinct().Count() == ids.Count)
            .WithMessage("قائمة ملفات الأدلة تحتوي على معرّف غير صالح أو مكرر.");
    }
}
