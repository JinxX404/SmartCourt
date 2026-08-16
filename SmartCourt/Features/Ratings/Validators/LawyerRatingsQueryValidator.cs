using FluentValidation;
using SmartCourt.Features.Ratings.DTOs;

namespace SmartCourt.Features.Ratings.Validators;

public sealed class LawyerRatingsQueryValidator : AbstractValidator<LawyerRatingsQuery>
{
    public LawyerRatingsQueryValidator()
    {
        RuleFor(query => query.Page)
            .GreaterThanOrEqualTo(1)
            .WithMessage("رقم الصفحة يجب أن يكون 1 أو أكبر.");

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 100)
            .WithMessage("حجم الصفحة يجب أن يكون بين 1 و100.");
    }
}
