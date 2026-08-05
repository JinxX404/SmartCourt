using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Infrastructure.Providers.Jobs;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Milestones.Events;

public sealed class MilestoneSchedulingOutboxHandler
    : IOutboxEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    private readonly ApplicationDbContext _dbContext;
    private readonly IContractJobScheduler _jobScheduler;

    public MilestoneSchedulingOutboxHandler(
        ApplicationDbContext dbContext,
        IContractJobScheduler jobScheduler)
    {
        _dbContext = dbContext;
        _jobScheduler = jobScheduler;
    }

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.MilestoneSubmitted,
        ContractPaymentEventTypes.MilestoneAccepted,
        ContractPaymentEventTypes.MilestoneAutoAccepted
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        switch (message.EventType)
        {
            case ContractPaymentEventTypes.MilestoneSubmitted:
                await ScheduleAutoAcceptAsync(message, cancellationToken);
                break;
            case ContractPaymentEventTypes.MilestoneAccepted:
            case ContractPaymentEventTypes.MilestoneAutoAccepted:
                await ScheduleHoldReleaseAsync(message, cancellationToken);
                break;
        }
    }

    private async Task ScheduleAutoAcceptAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<MilestoneSubmissionEventPayload>(message);
        var milestone = await _dbContext.Milestones
            .SingleOrDefaultAsync(
                item => item.Id == payload.MilestoneId,
                cancellationToken);
        if (milestone is null
            || milestone.Status != MilestoneStatus.Submitted
            || milestone.SubmissionVersion != payload.SubmissionVersion
            || !milestone.AutoAcceptEligibleAt.HasValue
            || !string.IsNullOrWhiteSpace(milestone.AutoAcceptJobId))
        {
            return;
        }

        var submissionMatches = await _dbContext.MilestoneSubmissions
            .AnyAsync(
                submission =>
                    submission.MilestoneId == payload.MilestoneId
                    && submission.EscrowHoldId == payload.EscrowHoldId
                    && submission.Version == payload.SubmissionVersion,
                cancellationToken);
        if (!submissionMatches)
        {
            return;
        }

        var jobId = await _jobScheduler.ScheduleAutoAcceptAsync(
            payload.MilestoneId,
            payload.EscrowHoldId,
            payload.SubmissionVersion,
            milestone.AutoAcceptEligibleAt.Value,
            cancellationToken);
        milestone.AutoAcceptJobId = jobId;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task ScheduleHoldReleaseAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var identifiers = message.EventType
            == ContractPaymentEventTypes.MilestoneAutoAccepted
            ? DeserializeAutoAccepted(message)
            : DeserializeAccepted(message);
        var milestone = await _dbContext.Milestones
            .AsNoTracking()
            .SingleOrDefaultAsync(
                item => item.Id == identifiers.MilestoneId,
                cancellationToken);
        if (milestone is null
            || milestone.Status != MilestoneStatus.AcceptedHold
            || !milestone.HoldExpiresAt.HasValue)
        {
            return;
        }

        await _jobScheduler.ScheduleReleaseExpiredHoldAsync(
            identifiers.EscrowHoldId,
            milestone.HoldExpiresAt.Value,
            cancellationToken);
    }

    private static MilestoneAcceptanceEventPayload DeserializeAutoAccepted(
        OutboxMessage message)
    {
        var payload = Deserialize<MilestoneAutoAcceptedEventPayload>(message);
        return new MilestoneAcceptanceEventPayload(
            payload.MilestoneId,
            payload.EscrowHoldId);
    }

    private static MilestoneAcceptanceEventPayload DeserializeAccepted(
        OutboxMessage message)
    {
        return Deserialize<MilestoneAcceptanceEventPayload>(message);
    }

    private static T Deserialize<T>(OutboxMessage message)
    {
        return JsonSerializer.Deserialize<T>(
                message.Payload,
                SerializerOptions)
            ?? throw new InvalidOperationException(
                $"Outbox payload for {message.EventType} is invalid.");
    }
}
