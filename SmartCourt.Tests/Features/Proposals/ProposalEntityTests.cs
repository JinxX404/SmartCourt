using SmartCourt.Common.Exceptions;
using SmartCourt.Entities;
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

    [Fact]
    public void NewProposal_ExpiresAfterThreeDays()
    {
        var proposal = CreateProposal();

        Assert.Equal(
            proposal.CreatedAt.AddDays(3),
            proposal.ExpiresAt);
    }

    [Fact]
    public void Cancel_ReleasesPendingProposalWithAuditDetails()
    {
        var proposal = CreateProposal();
        var actor = proposal.ClientUserId;
        var cancelledAt = proposal.CreatedAt.AddHours(2);

        proposal.Cancel("  Client changed direction  ", actor, cancelledAt);

        Assert.Equal(ProposalStatus.Cancelled, proposal.Status);
        Assert.Equal("Client changed direction", proposal.DecisionReason);
        Assert.Equal(actor, proposal.ClosedByUserId);
        Assert.Equal(cancelledAt, proposal.ClosedAt);
    }

    [Fact]
    public void Terminate_OnlyAllowsAcceptedProposal()
    {
        var proposal = CreateProposal();

        Assert.Throws<BusinessException>(() => proposal.Terminate(
            "No agreement",
            proposal.ClientUserId,
            proposal.CreatedAt.AddHours(1)));

        proposal.Accept(proposal.CreatedAt.AddMinutes(30));
        proposal.Terminate(
            "No agreement",
            proposal.ClientUserId,
            proposal.CreatedAt.AddHours(1));

        Assert.Equal(ProposalStatus.Terminated, proposal.Status);
    }

    [Fact]
    public void Accept_FailsAtOrAfterExpirationDeadline()
    {
        var proposal = CreateProposal();

        Assert.Throws<BusinessException>(() =>
            proposal.Accept(proposal.ExpiresAt));
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

        proposal.Case = new SmartCourt.Entities.Case { Id = caseId, ClientId = proposal.ClientUserId, Title = "Case title", Description = "Case description", City = null, SubmittedAt = new DateTime(2026, 7, 30, 8, 0, 0, DateTimeKind.Utc), Status = CaseStatus.Submitted };

        return proposal;
    }
}

