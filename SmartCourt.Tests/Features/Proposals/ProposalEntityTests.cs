using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Cases.Entities;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Proposals.Entities;
using SmartCourt.Features.Proposals.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Proposals;

public sealed class ProposalEntityTests
{
    [Fact]
    public void Accept_TransitionsPendingProposal()
    {
        var proposal = CreateProposal();
        var acceptedAt = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);

        proposal.Accept(acceptedAt);

        Assert.Equal(ProposalStatus.Accepted, proposal.Status);
        Assert.Null(proposal.DecisionReason);
        Assert.Equal(acceptedAt, proposal.RespondedAt);
        Assert.Equal(acceptedAt, proposal.UpdatedAt);
    }

    [Fact]
    public void Reject_TransitionsPendingProposal()
    {
        var proposal = CreateProposal();
        var rejectedAt = new DateTime(2026, 7, 30, 11, 0, 0, DateTimeKind.Utc);

        proposal.Reject("  Not a fit  ", rejectedAt);

        Assert.Equal(ProposalStatus.Rejected, proposal.Status);
        Assert.Equal("Not a fit", proposal.DecisionReason);
        Assert.Equal(rejectedAt, proposal.RespondedAt);
        Assert.Equal(rejectedAt, proposal.UpdatedAt);
    }

    [Fact]
    public void Decision_FailsWhenProposalIsNoLongerPending()
    {
        var proposal = CreateProposal();
        var acceptedAt = new DateTime(2026, 7, 30, 10, 0, 0, DateTimeKind.Utc);
        proposal.Accept(acceptedAt);

        var exception = Assert.Throws<BusinessException>(() =>
            proposal.Reject("Too late", acceptedAt.AddMinutes(1)));

        Assert.Contains("pending", exception.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static Proposal CreateProposal()
    {
        var caseId = Guid.NewGuid();
        var proposal = new Proposal(
            Guid.NewGuid(),
            caseId,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "Please review this proposal.",
            new DateTime(2026, 7, 30, 9, 0, 0, DateTimeKind.Utc));

        proposal.LegalCase = new LegalCase(
            caseId,
            proposal.ClientUserId,
            "Case title",
            "Case description",
            null,
            new DateTime(2026, 7, 30, 8, 0, 0, DateTimeKind.Utc))
        {
            Status = CaseStatus.Submitted
        };

        return proposal;
    }
}
