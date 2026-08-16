using FluentValidation;
using SmartCourt.Features.Users.Lawyers.Dashboard.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Dashboard.Validators;

public sealed class LawyerActivityQueryValidator : AbstractValidator<LawyerActivityQuery>
{
    public LawyerActivityQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("يجب أن يكون رقم الصفحة 1 على الأقل.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("يجب أن يكون حجم الصفحة بين 1 و 50.");
    }
}
