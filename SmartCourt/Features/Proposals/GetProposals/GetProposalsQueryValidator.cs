using FluentValidation;

namespace SmartCourt.Features.Proposals.GetProposals;

public sealed class GetProposalsQueryValidator : AbstractValidator<GetProposalsQuery>
{
    public GetProposalsQueryValidator()
    {
        RuleFor(query => query.Scope).IsInEnum();
        RuleFor(query => query.LegalCaseId)
            .NotEmpty()
            .When(query => query.Scope == ProposalListScope.ClientCase);
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 50);
        RuleFor(query => query.Search).MaximumLength(100);
        RuleForEach(query => query.Statuses).IsInEnum();
    }
}
