using FluentValidation;

namespace SmartCourt.Features.Proposals.RejectProposal;

public sealed class RejectProposalCommandValidator
    : AbstractValidator<RejectProposalCommand>
{
    public RejectProposalCommandValidator()
    {
        RuleFor(command => command.Reason)
            .NotEmpty()
            .MaximumLength(1_000);
    }
}
