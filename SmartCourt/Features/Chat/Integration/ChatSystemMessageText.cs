namespace SmartCourt.Features.Chat.Integration;

internal static class ChatSystemMessageText
{
    public static string For(ContractConversationMessageType type)
    {
        return type switch
        {
            ContractConversationMessageType.ContractCreated =>
                "Contract draft was created.",
            ContractConversationMessageType.ContractAccepted =>
                "Contract draft was accepted.",
            ContractConversationMessageType.ContractActivated =>
                "Contract is now active.",
            ContractConversationMessageType.ContractCompleted =>
                "Contract was completed.",
            ContractConversationMessageType.MilestoneReadyForFunding =>
                "Milestone is ready for funding.",
            ContractConversationMessageType.MilestoneFundingStarted =>
                "Milestone funding started.",
            ContractConversationMessageType.MilestoneFunded =>
                "Milestone was funded.",
            ContractConversationMessageType.MilestoneFundingFailed =>
                "Milestone funding failed.",
            ContractConversationMessageType.MilestoneSubmitted =>
                "Milestone work was submitted.",
            ContractConversationMessageType.MilestoneAutoAccepted =>
                "Milestone was accepted automatically.",
            ContractConversationMessageType.MilestoneAccepted =>
                "Milestone was accepted.",
            ContractConversationMessageType.MilestoneChangesRequested =>
                "Milestone changes were requested.",
            ContractConversationMessageType.MilestoneChangeRequestApproved =>
                "Milestone change request was approved.",
            ContractConversationMessageType.MilestoneChangeRequestRejected =>
                "Milestone change request was rejected.",
            ContractConversationMessageType.MilestoneChangeRequestCancelled =>
                "Milestone change request was cancelled.",
            ContractConversationMessageType.DisputeOpened =>
                "A dispute was opened.",
            ContractConversationMessageType.DisputeAssigned =>
                "A moderator was assigned to the dispute.",
            ContractConversationMessageType.DisputeResolved =>
                "Dispute was resolved.",
            ContractConversationMessageType.DisputeClosed =>
                "Dispute was closed.",
            ContractConversationMessageType.FundsReleased =>
                "Funds were released.",
            ContractConversationMessageType.FundsRefunded =>
                "Funds were refunded.",
            ContractConversationMessageType.ContractTerminated =>
                "Contract was terminated.",
            _ => "Conversation was updated."
        };
    }
}
