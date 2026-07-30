using FluentValidation;

namespace SmartCourt.Features.Proposals.CreateProposal;

public sealed class CreateProposalCommandValidator : AbstractValidator<CreateProposalCommand>
{
    public CreateProposalCommandValidator()
    {
        RuleFor(command => command.LegalCaseId).NotEmpty();
        RuleFor(command => command.LawyerUserId).NotEmpty();
        RuleFor(command => command.Message)
            .NotEmpty()
            .MaximumLength(2_000);
    }
}
