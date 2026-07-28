namespace SmartCourt.Infrastructure.Providers.Events;

public sealed record MilestoneSubmissionEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    int SubmissionVersion);

public sealed record MilestoneAutoAcceptedEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    int SubmissionVersion);

public sealed record MilestoneAcceptanceEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId);

public sealed record MilestoneChangeRequestEventPayload(
    Guid MilestoneId,
    Guid ChangeRequestId,
    string Status);

public sealed record ContractPaymentAggregateEventPayload(
    Guid EntityId);
