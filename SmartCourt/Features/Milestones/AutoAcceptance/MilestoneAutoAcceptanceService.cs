using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Domain;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.FundingVerification;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones;

public sealed class MilestoneAutoAcceptanceService(
    ApplicationDbContext dbContext,
    IMilestoneFundingVerifier fundingVerifier,
    IOutboxWriter outboxWriter,
    TimeProvider timeProvider,
    ILogger<MilestoneAutoAcceptanceService> logger)
    : IMilestoneAutoAcceptanceService
{
    public async Task<JobExecutionResult> AutoAcceptAsync(
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion,
        CancellationToken cancellationToken)
    {
        if (milestoneId == Guid.Empty
            || escrowHoldId == Guid.Empty
            || submissionVersion <= 0)
        {
            return NoOp(
                "InvalidAutoAcceptJobArguments",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        await using var transaction = dbContext.Database.IsRelational()
            ? await dbContext.Database.BeginTransactionAsync(
                cancellationToken)
            : null;
        var milestone = await dbContext.Milestones
            .SingleOrDefaultAsync(
                item => item.Id == milestoneId,
                cancellationToken);
        if (milestone is null)
        {
            return NoOp(
                "MilestoneNotFound",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (milestone.Type != MilestoneType.Standard
            || milestone.Status != MilestoneStatus.Submitted)
        {
            return NoOp(
                "MilestoneNoLongerSubmitted",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        var now = timeProvider.GetUtcNow();
        if (!milestone.FundedAt.HasValue)
        {
            return NoOp(
                "MilestoneFundingTimestampMissing",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (!milestone.AutoAcceptEligibleAt.HasValue
            || milestone.AutoAcceptEligibleAt.Value > now)
        {
            return NoOp(
                "AutoAcceptDeadlineNotElapsed",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (milestone.SubmissionVersion != submissionVersion)
        {
            return NoOp(
                "SubmissionVersionIsStale",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (milestone.AcceptedAt.HasValue
            || milestone.AcceptanceSource.HasValue
            || milestone.HoldStartsAt.HasValue
            || milestone.HoldExpiresAt.HasValue
            || milestone.ReleasedAt.HasValue
            || milestone.RefundedAt.HasValue)
        {
            return NoOp(
                "MilestoneReviewWasSuperseded",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        var submission = await dbContext.MilestoneSubmissions
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item =>
                    item.MilestoneId == milestoneId
                    && item.Version == submissionVersion,
                cancellationToken);
        if (submission is null
            || submission.EscrowHoldId != escrowHoldId)
        {
            return NoOp(
                "SubmissionDoesNotMatchJobHold",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        var hasPendingChangeRequest =
            await dbContext.MilestoneChangeRequests.AnyAsync(
                item =>
                    item.MilestoneId == milestoneId
                    && item.Status == ChangeRequestStatus.Pending,
                cancellationToken);
        if (hasPendingChangeRequest)
        {
            return NoOp(
                "PendingMilestoneChangeRequestExists",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        VerifiedMilestoneFunding verifiedFunding;
        try
        {
            verifiedFunding = await fundingVerifier.VerifyAsync(
                milestoneId,
                FundingVerificationOperation.AutomaticAcceptance,
                cancellationToken);
        }
        catch (BusinessException)
        {
            return NoOp(
                "MilestoneFundingChainIsInvalid",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (verifiedFunding.EscrowHoldId != escrowHoldId
            || verifiedFunding.ContractId != milestone.ContractId
            || verifiedFunding.GrossAmount != milestone.Amount
            || !string.Equals(
                verifiedFunding.Currency,
                "EGP",
                StringComparison.Ordinal))
        {
            return NoOp(
                "MilestoneFundingChainDoesNotMatchJob",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        var hold = await dbContext.EscrowHolds.SingleOrDefaultAsync(
            item =>
                item.Id == escrowHoldId
                && item.MilestoneId == milestoneId
                && item.ContractId == milestone.ContractId,
            cancellationToken);
        if (hold is null || hold.Status != EscrowHoldStatus.Funded)
        {
            return NoOp(
                "EscrowHoldIsNotFunded",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        var holdExpiresAt = now.AddDays(14);
        var correlationId = Guid.NewGuid();
        MilestoneTransitionGuard.EnsureCanTransition(
            milestone.Status,
            MilestoneStatus.AcceptedHold);
        milestone.Status = MilestoneStatus.AcceptedHold;
        milestone.AcceptedAt = now;
        milestone.AcceptanceSource =
            MilestoneAcceptanceSource.Automatic;
        milestone.HoldStartsAt = now;
        milestone.HoldExpiresAt = holdExpiresAt;
        milestone.AutoAcceptEligibleAt = null;
        milestone.AutoAcceptJobId = null;
        milestone.UpdatedAt = now;
        hold.HoldStartsAt = now;
        hold.HoldExpiresAt = holdExpiresAt;
        hold.UpdatedAt = now;
        dbContext.MilestoneStateHistories.Add(
            MilestoneStateHistoryFactory.Create(
                Guid.NewGuid(),
                milestone.Id,
                MilestoneStatus.Submitted,
                MilestoneStatus.AcceptedHold,
                ContractPaymentEventTypes.MilestoneAutoAccepted,
                actorUserId: null,
                "انتهت مدة مراجعة العميل وتم قبول تسليم المرحلة تلقائيًا.",
                correlationId,
                now));
        await outboxWriter.EnqueueAsync(
            new OutboxEvent(
                ContractPaymentEventTypes.MilestoneAutoAccepted,
                1,
                new MilestoneAutoAcceptedEventPayload(
                    milestone.Id,
                    hold.Id,
                    submissionVersion),
                "Milestone",
                milestone.Id,
                correlationId),
            cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NoOp(
                "MilestoneChangedConcurrently",
                milestoneId,
                escrowHoldId,
                submissionVersion);
        }

        if (transaction is not null)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        logger.LogInformation(
            "Milestone auto-accept completed for milestone {MilestoneId}, hold {EscrowHoldId}, submission version {SubmissionVersion}.",
            milestoneId,
            escrowHoldId,
            submissionVersion);
        return JobExecutionResult.Completed("MilestoneAutoAccepted");
    }

    private JobExecutionResult NoOp(
        string reason,
        Guid milestoneId,
        Guid escrowHoldId,
        int submissionVersion)
    {
        logger.LogInformation(
            "Milestone auto-accept no-op for milestone {MilestoneId}, hold {EscrowHoldId}, submission version {SubmissionVersion}. Reason: {Reason}.",
            milestoneId,
            escrowHoldId,
            submissionVersion,
            reason);
        return JobExecutionResult.NoOp(reason);
    }
}
