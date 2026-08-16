using FluentValidation;

namespace SmartCourt.Features.Proposals.UpdateProposal;

public sealed class UpdateProposalCommandValidator
    : AbstractValidator<UpdateProposalCommand>
{
    public UpdateProposalCommandValidator()
    {
        RuleFor(command => command.ProposalId).NotEmpty();
        RuleFor(command => command.Message)
            .NotEmpty()
            .MaximumLength(2_000);
    }
}
