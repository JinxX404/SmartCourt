namespace SmartCourt.Infrastructure.Providers.Events;

public static class ContractPaymentEventTypes
{
    public const string ProposalCreated = "ProposalCreated";
    public const string ProposalAccepted = "ProposalAccepted";
    public const string ProposalRejected = "ProposalRejected";
    public const string ContractCreated = "ContractCreated";
    public const string ContractDraftUpdated = "ContractDraftUpdated";
    public const string ContractAccepted = "ContractAccepted";
    public const string ContractActivationRequested =
        "ContractActivationRequested";
    public const string ContractActivated = "ContractActivated";
    public const string ContractCompleted = "ContractCompleted";
    public const string ContractTerminationRequested =
        "ContractTerminationRequested";
    public const string MilestoneCreated = "MilestoneCreated";
    public const string MilestoneDraftUpdated = "MilestoneDraftUpdated";
    public const string MilestoneAcceptanceRecorded =
        "MilestoneAcceptanceRecorded";
    public const string MilestoneApproved = "MilestoneApproved";
    public const string MilestoneReadyForFunding = "MilestoneReadyForFunding";
    public const string MilestoneFundingStarted = "MilestoneFundingStarted";
    public const string MilestoneFunded = "MilestoneFunded";
    public const string MilestoneFundingFailed = "MilestoneFundingFailed";
    public const string MilestoneSubmitted = "MilestoneSubmitted";
    public const string MilestoneAutoAccepted = "MilestoneAutoAccepted";
    public const string MilestoneAccepted = "MilestoneAccepted";
    public const string MilestoneChangesRequested = "MilestoneChangesRequested";
    public const string MilestoneChangeRequestCreated =
        "MilestoneChangeRequestCreated";
    public const string MilestoneChangeRequestApproved =
        "MilestoneChangeRequestApproved";
    public const string MilestoneChangeRequestRejected =
        "MilestoneChangeRequestRejected";
    public const string MilestoneChangeRequestCancelled =
        "MilestoneChangeRequestCancelled";
    public const string FundsReleased = "FundsReleased";
    public const string FundsRefunded = "FundsRefunded";
    public const string WithdrawalCompleted = "WithdrawalCompleted";
    public const string WithdrawalFailed = "WithdrawalFailed";
    public const string WithdrawalDelayed = "WithdrawalDelayed";
    public const string WalletAdjusted = "WalletAdjusted";
    public const string DisputeOpened = "DisputeOpened";
    public const string DisputeAssigned = "DisputeAssigned";
    public const string DisputeResolved = "DisputeResolved";
    public const string DisputeClosed = "DisputeClosed";
    public const string ContractTerminated = "ContractTerminated";
}
