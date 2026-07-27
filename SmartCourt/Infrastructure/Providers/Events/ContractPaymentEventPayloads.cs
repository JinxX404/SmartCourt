namespace SmartCourt.Infrastructure.Providers.Events;

public sealed record MilestoneSubmissionEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    int SubmissionVersion);

public sealed record MilestoneAutoAcceptedEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    int SubmissionVersion);

public sealed record ContractPaymentAggregateEventPayload(
    Guid EntityId);
