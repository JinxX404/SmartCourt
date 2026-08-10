using FluentValidation;

namespace SmartCourt.Features.Proposals.GetProposals;

public sealed class GetProposalsQueryValidator : AbstractValidator<GetProposalsQuery>
{
    public GetProposalsQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(100);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
