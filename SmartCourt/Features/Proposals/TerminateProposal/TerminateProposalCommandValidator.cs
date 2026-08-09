using FluentValidation;

namespace SmartCourt.Features.Proposals.TerminateProposal;

public sealed class TerminateProposalCommandValidator
    : AbstractValidator<TerminateProposalCommand>
{
    public TerminateProposalCommandValidator()
    {
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.Reason).NotEmpty().MaximumLength(1_000);
    }
}
