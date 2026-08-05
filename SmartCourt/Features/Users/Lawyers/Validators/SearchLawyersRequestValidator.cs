using FluentValidation;
using SmartCourt.Features.Users.Lawyers.DTOs;

namespace SmartCourt.Features.Users.Lawyers.Validators;

public class SearchLawyersRequestValidator : AbstractValidator<SearchLawyersRequest>
{
    public SearchLawyersRequestValidator()
    {
        RuleFor(x => x.MinRating)
            .InclusiveBetween(0m, 5m)
            .When(x => x.MinRating.HasValue)
            .WithMessage("يجب أن يكون الحد الأدنى للتقييم بين 0 و 5.");

        RuleFor(x => x.Level)
            .IsInEnum()
            .When(x => x.Level.HasValue)
            .WithMessage("مستوى المحامي غير صالح.");

        RuleFor(x => x.Specialization)
            .IsInEnum()
            .When(x => x.Specialization.HasValue)
            .WithMessage("التخصص غير صالح.");

        RuleFor(x => x.SortBy)
            .IsInEnum()
            .WithMessage("خيار الترتيب غير صالح.");

        RuleFor(x => x.SortDirection)
            .IsInEnum()
            .WithMessage("اتجاه الترتيب غير صالح.");

        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("رقم الصفحة يجب أن يكون 1 على الأقل.");

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 50)
            .WithMessage("حجم الصفحة يجب أن يكون بين 1 و 50.");
    }
}
