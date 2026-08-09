using System;
using FluentValidation;
using SmartCourt.Features.Case.GetUserCases.DTOs;

namespace SmartCourt.Features.Case.GetUserCases.Validators;

public sealed class GetUserCasesQueryValidator : AbstractValidator<GetUserCasesQuery>
{
    public GetUserCasesQueryValidator()
    {
        RuleFor(query => query.Status)
            .Must(value => !value.HasValue || Enum.IsDefined(value.Value))
            .WithMessage("حالة القضية المطلوبة غير صالحة.");

        RuleFor(query => query.Page)
            .GreaterThan(0)
            .WithMessage("رقم الصفحة يجب أن يكون أكبر من صفر.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يتراوح بين 1 و100.");
    }
}
