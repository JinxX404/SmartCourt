namespace SmartCourt.Infrastructure.Providers.Events;

public static class ContractPaymentEventTypes
{
    public const string ContractCreated = "ContractCreated";
    public const string ContractAccepted = "ContractAccepted";
    public const string ContractActivated = "ContractActivated";
    public const string MilestoneReadyForFunding = "MilestoneReadyForFunding";
    public const string MilestoneFundingStarted = "MilestoneFundingStarted";
    public const string MilestoneFunded = "MilestoneFunded";
    public const string MilestoneFundingFailed = "MilestoneFundingFailed";
    public const string MilestoneSubmitted = "MilestoneSubmitted";
    public const string MilestoneAutoAccepted = "MilestoneAutoAccepted";
    public const string MilestoneAccepted = "MilestoneAccepted";
    public const string MilestoneChangesRequested = "MilestoneChangesRequested";
    public const string FundsReleased = "FundsReleased";
    public const string FundsRefunded = "FundsRefunded";
    public const string DisputeOpened = "DisputeOpened";
    public const string DisputeAssigned = "DisputeAssigned";
    public const string DisputeResolved = "DisputeResolved";
    public const string DisputeClosed = "DisputeClosed";
    public const string ContractTerminated = "ContractTerminated";
}
