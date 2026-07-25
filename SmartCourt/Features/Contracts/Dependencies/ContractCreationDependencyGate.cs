using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Integration;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Users.Integration;

namespace SmartCourt.Features.Contracts.Dependencies;

public sealed class ContractCreationDependencyGate(
    IProposalContractAccessService proposalService,
    ICaseContractAccessService caseService,
    IContractUserEligibilityService userEligibilityService)
    : IContractCreationDependencyGate
{
    public async Task<ContractCreationFacts> VerifyAsync(
        Guid proposalId,
        Guid actorUserId,
        CancellationToken cancellationToken)
    {
        if (proposalId == Guid.Empty)
        {
            throw new BusinessException("A proposal is required to create a contract.");
        }

        if (actorUserId == Guid.Empty)
        {
            throw new BusinessException("An authenticated lawyer is required to create a contract.");
        }

        var proposal = await proposalService.FindAcceptedForContractAsync(
            proposalId,
            cancellationToken);

        if (proposal is null)
        {
            throw new BusinessException("The proposal does not exist or is not accepted.");
        }

        if (proposal.ProposalId != proposalId)
        {
            throw new BusinessException("The proposal service returned inconsistent contract facts.");
        }

        if (proposal.LawyerUserId != actorUserId)
        {
            throw new BusinessException("Only the accepted proposal's lawyer can create the contract.");
        }

        var legalCase = await caseService.FindEligibleForContractAsync(
            proposal.LegalCaseId,
            cancellationToken);

        if (legalCase is null)
        {
            throw new BusinessException("The case is not eligible for contract creation.");
        }

        if (legalCase.LegalCaseId != proposal.LegalCaseId
            || legalCase.ClientUserId != proposal.ClientUserId)
        {
            throw new BusinessException("The accepted proposal does not match the eligible case owner.");
        }

        var clientEligibility = await userEligibilityService.FindEligibilityAsync(
            proposal.ClientUserId,
            cancellationToken);
        if (clientEligibility is null
            || clientEligibility.UserId != proposal.ClientUserId
            || !clientEligibility.IsActive
            || !clientEligibility.CanActAsClient)
        {
            throw new BusinessException("The proposal client is not eligible to enter a contract.");
        }

        var lawyerEligibility = await userEligibilityService.FindEligibilityAsync(
            proposal.LawyerUserId,
            cancellationToken);
        if (lawyerEligibility is null
            || lawyerEligibility.UserId != proposal.LawyerUserId
            || !lawyerEligibility.IsActive
            || !lawyerEligibility.CanActAsLawyer)
        {
            throw new BusinessException("The proposal lawyer is not eligible to enter a contract.");
        }

        return new ContractCreationFacts(
            proposal.ProposalId,
            proposal.LegalCaseId,
            proposal.ClientUserId,
            proposal.LawyerUserId);
    }
}
