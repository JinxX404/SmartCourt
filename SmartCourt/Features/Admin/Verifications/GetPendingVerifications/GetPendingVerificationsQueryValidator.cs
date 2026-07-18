using FluentValidation;

namespace SmartCourt.Features.Admin.Verifications.GetPendingVerifications;

public sealed class GetPendingVerificationsQueryValidator : AbstractValidator<GetPendingVerificationsQuery>
{
    public GetPendingVerificationsQueryValidator()
    {
        RuleFor(query => query.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(query => query.PageSize)
            .InclusiveBetween(1, 50);

        RuleFor(query => query.Search)
            .MaximumLength(100)
            .When(query => !string.IsNullOrWhiteSpace(query.Search));
    }
}
