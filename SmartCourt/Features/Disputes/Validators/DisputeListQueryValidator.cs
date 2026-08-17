using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class DisputeListQueryValidator
    : AbstractValidator<DisputeListQuery>
{
    public DisputeListQueryValidator()
    {
        RuleFor(query => query.ContractId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("معرّف العقد المطلوب للتصفية غير صالح.");
        RuleFor(query => query.MilestoneId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("معرّف المرحلة المطلوب للتصفية غير صالح.");
        RuleFor(query => query.Status)
            .Must(value => !value.HasValue || Enum.IsDefined(value.Value))
            .WithMessage("حالة النزاع المطلوبة للتصفية غير صالحة.");
        RuleFor(query => query.Category)
            .Must(value => !value.HasValue || Enum.IsDefined(value.Value))
            .WithMessage("تصنيف النزاع المطلوب للتصفية غير صالح.");
        RuleFor(query => query.RaisedByUserId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("معرّف منشئ النزاع المطلوب للتصفية غير صالح.");
        RuleFor(query => query.AssignedModeratorUserId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("معرّف المشرف المطلوب للتصفية غير صالح.");
        RuleFor(query => query.ToDate)
            .GreaterThanOrEqualTo(query => query.FromDate!.Value)
            .When(query => query.FromDate.HasValue && query.ToDate.HasValue)
            .WithMessage("تاريخ النهاية يجب أن يكون بعد تاريخ البداية أو يساويه.");
        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("رقم الصفحة يجب أن يكون أكبر من صفر.");
        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يتراوح بين 1 و100.");
    }
}

