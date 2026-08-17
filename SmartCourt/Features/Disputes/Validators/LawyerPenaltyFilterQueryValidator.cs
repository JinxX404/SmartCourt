using FluentValidation;
using SmartCourt.Features.Disputes.DTOs;

namespace SmartCourt.Features.Disputes.Validators;

public sealed class LawyerPenaltyFilterQueryValidator
    : AbstractValidator<LawyerPenaltyFilterQuery>
{
    public LawyerPenaltyFilterQueryValidator()
    {
        RuleFor(query => query.LawyerUserId)
            .Must(value => !value.HasValue || value.Value != Guid.Empty)
            .WithMessage("معرّف المحامي المطلوب للتصفية غير صالح.");

        RuleFor(query => query.PenaltyType)
            .Must(value => !value.HasValue || Enum.IsDefined(value.Value))
            .WithMessage("نوع العقوبة المطلوب للتصفية غير صالح.");

        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("رقم الصفحة يجب أن يكون أكبر من صفر.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يتراوح بين 1 و100.");
    }
}
