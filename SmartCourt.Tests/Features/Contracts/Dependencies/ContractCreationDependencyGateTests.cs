using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Integration;
using SmartCourt.Features.Contracts.Dependencies;
using SmartCourt.Features.Proposals.Integration;
using SmartCourt.Features.Users.Integration;
using SmartCourt.Tests.TestDoubles.ContractAndPayment;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts.Dependencies;

public sealed class ContractCreationDependencyGateTests
{
    [Fact]
    public async Task VerifyAsync_ReturnsAuthoritativeFacts_WhenAllOwningSlicesApprove()
    {
        var context = CreateEligibleContext();

        var result = await context.Gate.VerifyAsync(
            context.ProposalId,
            context.LawyerUserId,
            CancellationToken.None);

        Assert.Equal(context.ProposalId, result.ProposalId);
        Assert.Equal(context.LegalCaseId, result.LegalCaseId);
        Assert.Equal(context.ClientUserId, result.ClientUserId);
        Assert.Equal(context.LawyerUserId, result.LawyerUserId);
        Assert.Equal(1, context.ProposalService.CallCount);
        Assert.Equal(1, context.CaseService.CallCount);
        Assert.Equal(2, context.UserService.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_RejectsProposalThatOwningSliceDoesNotConfirmAsAccepted()
    {
        var context = CreateEligibleContext();
        context.ProposalService.AcceptedProposal = null;

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            context.Gate.VerifyAsync(
                context.ProposalId,
                context.LawyerUserId,
                CancellationToken.None));

        Assert.Equal(
            "العرض غير موجود أو لم تتم الموافقة عليه.",
            exception.Message);
        Assert.Equal(0, context.CaseService.CallCount);
        Assert.Equal(0, context.UserService.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_RejectsActorWhoIsNotAcceptedProposalLawyer()
    {
        var context = CreateEligibleContext();

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            context.Gate.VerifyAsync(
                context.ProposalId,
                Guid.NewGuid(),
                CancellationToken.None));

        Assert.Equal(
            "محامي العرض المقبول فقط هو من يمكنه إنشاء العقد.",
            exception.Message);
        Assert.Equal(0, context.CaseService.CallCount);
    }

    [Fact]
    public async Task VerifyAsync_RejectsCaseOwnerMismatch()
    {
        var context = CreateEligibleContext();
        context.CaseService.EligibleCase = new CaseContractEligibilityFacts(
            context.LegalCaseId,
            Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            context.Gate.VerifyAsync(
                context.ProposalId,
                context.LawyerUserId,
                CancellationToken.None));

        Assert.Equal(
            "العرض المقبول لا يطابق مالك القضية المؤهلة.",
            exception.Message);
        Assert.Equal(0, context.UserService.CallCount);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task VerifyAsync_RejectsIneligibleParticipant(bool rejectClient)
    {
        var context = CreateEligibleContext();
        var userId = rejectClient ? context.ClientUserId : context.LawyerUserId;
        var current = context.UserService.Results[userId];
        context.UserService.Results[userId] = rejectClient
            ? current with { CanActAsClient = false }
            : current with { CanActAsLawyer = false };

        var exception = await Assert.ThrowsAsync<BusinessException>(() =>
            context.Gate.VerifyAsync(
                context.ProposalId,
                context.LawyerUserId,
                CancellationToken.None));

        Assert.Equal(
            rejectClient
                ? "صاحب العرض غير مؤهل لإبرام العقد بصفته عميلاً."
                : "محامي العرض غير مؤهل لإبرام العقد.",
            exception.Message);
    }

    private static TestContext CreateEligibleContext()
    {
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var clientUserId = Guid.NewGuid();
        var lawyerUserId = Guid.NewGuid();

        var proposalService = new FakeProposalContractAccessService
        {
            AcceptedProposal = new AcceptedProposalContractFacts(
                proposalId,
                legalCaseId,
                clientUserId,
                lawyerUserId)
        };
        var caseService = new FakeCaseContractAccessService
        {
            EligibleCase = new CaseContractEligibilityFacts(legalCaseId, clientUserId)
        };
        var userService = new FakeContractUserEligibilityService();
        userService.Results[clientUserId] = new ContractUserEligibilityFacts(
            clientUserId,
            IsActive: true,
            CanActAsClient: true,
            CanActAsLawyer: false,
            CanActAsModerator: false,
            CanActAsFinanceAdministrator: false,
            CanActAsSuperAdministrator: false);
        userService.Results[lawyerUserId] = new ContractUserEligibilityFacts(
            lawyerUserId,
            IsActive: true,
            CanActAsClient: false,
            CanActAsLawyer: true,
            CanActAsModerator: false,
            CanActAsFinanceAdministrator: false,
            CanActAsSuperAdministrator: false);

        var gate = new ContractCreationDependencyGate(
            proposalService,
            caseService,
            userService);

        return new TestContext(
            proposalId,
            legalCaseId,
            clientUserId,
            lawyerUserId,
            proposalService,
            caseService,
            userService,
            gate);
    }

    private sealed record TestContext(
        Guid ProposalId,
        Guid LegalCaseId,
        Guid ClientUserId,
        Guid LawyerUserId,
        FakeProposalContractAccessService ProposalService,
        FakeCaseContractAccessService CaseService,
        FakeContractUserEligibilityService UserService,
        ContractCreationDependencyGate Gate);
}
