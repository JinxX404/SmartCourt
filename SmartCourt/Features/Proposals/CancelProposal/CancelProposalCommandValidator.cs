using FluentValidation;

namespace SmartCourt.Features.Proposals.CancelProposal;

public sealed class CancelProposalCommandValidator
    : AbstractValidator<CancelProposalCommand>
{
    public CancelProposalCommandValidator()
    {
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1_000);
    }
}
