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
    MilestoneReadyForFunding = 3,
    MilestoneFunded = 4,
    MilestoneSubmitted = 5,
    MilestoneAccepted = 6,
    MilestoneChangesRequested = 7,
    DisputeOpened = 8,
    DisputeResolved = 9,
    FundsReleased = 10,
    FundsRefunded = 11,
    ContractTerminated = 12
}
