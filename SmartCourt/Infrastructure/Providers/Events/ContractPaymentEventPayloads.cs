namespace SmartCourt.Infrastructure.Providers.Events;

public sealed record ProposalEventPayload(
    Guid ProposalId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    Guid? ActorUserId = null,
    string? Reason = null);

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

public sealed record FundsReleasedEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    Guid PaymentTransactionId,
    decimal LawyerNetAmount,
    decimal PlatformFeeAmount);

public sealed record FundsRefundedEventPayload(
    Guid MilestoneId,
    Guid EscrowHoldId,
    Guid PaymentTransactionId,
    decimal ClientRefundAmount);

public sealed record MilestoneChangeRequestEventPayload(
    Guid MilestoneId,
    Guid ChangeRequestId,
    string Status);

public sealed record ContractPaymentAggregateEventPayload(
    Guid EntityId);

public sealed record ContractActivationRequestedEventPayload(
    Guid ContractId,
    Guid RequestedByUserId);

public sealed record ContractTerminatedEventPayload(
    Guid ContractId,
    Guid LegalCaseId,
    Guid TerminatedByUserId);
