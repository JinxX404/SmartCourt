namespace SmartCourt.Features.Chat.Integration;

public interface IContractConversationService
{
    Task AppendSystemMessageAsync(
        ContractConversationSystemMessage message,
        CancellationToken cancellationToken);
}

public sealed record ContractConversationSystemMessage(
    Guid EventId,
    Guid ProposalId,
    ContractConversationMessageType Type,
    Guid RelatedEntityId,
    DateTimeOffset OccurredAt);

public enum ContractConversationMessageType
{
    ContractCreated = 1,
    ContractAccepted = 2,
    ContractActivated = 3,
    ContractCompleted = 4,
    MilestoneReadyForFunding = 5,
    MilestoneFundingStarted = 6,
    MilestoneFunded = 7,
    MilestoneFundingFailed = 8,
    MilestoneSubmitted = 9,
    MilestoneAutoAccepted = 10,
    MilestoneAccepted = 11,
    MilestoneChangesRequested = 12,
    MilestoneChangeRequestApproved = 13,
    MilestoneChangeRequestRejected = 14,
    MilestoneChangeRequestCancelled = 15,
    DisputeOpened = 16,
    DisputeAssigned = 17,
    DisputeResolved = 18,
    DisputeClosed = 19,
    FundsReleased = 20,
    FundsRefunded = 21,
    ContractTerminated = 22
}
