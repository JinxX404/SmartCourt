using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Case.Integration;
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
            throw new BusinessException("معرّف العرض مطلوب لإنشاء العقد.");
        }

        if (actorUserId == Guid.Empty)
        {
            throw new BusinessException(
                "يجب تسجيل الدخول كمحامٍ لإنشاء العقد.");
        }

        var proposal = await proposalService.FindAcceptedForContractAsync(
            proposalId,
            cancellationToken);

        if (proposal is null)
        {
            throw new BusinessException(
                "العرض غير موجود أو لم تتم الموافقة عليه.");
        }

        if (proposal.ProposalId != proposalId)
        {
            throw new BusinessException(
                "بيانات العرض المسترجعة غير متطابقة مع العقد المطلوب.");
        }

        if (proposal.LawyerUserId != actorUserId)
        {
            throw new BusinessException(
                "محامي العرض المقبول فقط هو من يمكنه إنشاء العقد.");
        }

        var legalCase = await caseService.FindEligibleForContractAsync(
            proposal.LegalCaseId,
            cancellationToken);

        if (legalCase is null)
        {
            throw new BusinessException(
                "القضية غير مؤهلة لإنشاء عقد.");
        }

        if (legalCase.LegalCaseId != proposal.LegalCaseId
            || legalCase.ClientUserId != proposal.ClientUserId)
        {
            throw new BusinessException(
                "العرض المقبول لا يطابق مالك القضية المؤهلة.");
        }

        var clientEligibility = await userEligibilityService.FindEligibilityAsync(
            proposal.ClientUserId,
            cancellationToken);
        if (clientEligibility is null
            || clientEligibility.UserId != proposal.ClientUserId
            || !clientEligibility.IsActive
            || !clientEligibility.CanActAsClient)
        {
            throw new BusinessException(
                "صاحب العرض غير مؤهل لإبرام العقد بصفته عميلاً.");
        }

        var lawyerEligibility = await userEligibilityService.FindEligibilityAsync(
            proposal.LawyerUserId,
            cancellationToken);
        if (lawyerEligibility is null
            || lawyerEligibility.UserId != proposal.LawyerUserId
            || !lawyerEligibility.IsActive
            || !lawyerEligibility.CanActAsLawyer)
        {
            throw new BusinessException(
                "محامي العرض غير مؤهل لإبرام العقد.");
        }

        return new ContractCreationFacts(
            proposal.ProposalId,
            proposal.LegalCaseId,
            proposal.ClientUserId,
            proposal.LawyerUserId);
    }
}
