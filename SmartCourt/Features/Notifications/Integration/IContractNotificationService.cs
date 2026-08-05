namespace SmartCourt.Features.Notifications.Integration;

public interface IContractNotificationService
{
    Task PublishAsync(
        ContractNotification notification,
        CancellationToken cancellationToken);
}

public sealed record ContractNotification(
    Guid EventId,
    Guid RecipientUserId,
    ContractNotificationType Type,
    string RelatedEntityType,
    Guid RelatedEntityId);

public enum ContractNotificationType
{
    ContractCreated = 1,
    ContractAccepted = 2,
    ContractActivated = 3,
    MilestoneReadyForFunding = 4,
    MilestoneFundingStarted = 5,
    MilestoneFunded = 6,
    MilestoneFundingFailed = 7,
    MilestoneSubmitted = 8,
    MilestoneAutoAccepted = 9,
    MilestoneAccepted = 10,
    MilestoneChangesRequested = 11,
    FundsReleased = 12,
    FundsRefunded = 13,
    DisputeOpened = 14,
    DisputeAssigned = 15,
    DisputeResolved = 16,
    DisputeClosed = 17,
    ContractTerminated = 18
}
