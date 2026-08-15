using SmartCourt.Features.Chat.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Events;

public sealed class ContractConversationIntegrationOutboxHandler(
    ApplicationDbContext dbContext,
    IContractConversationService conversationService)
    : IOutboxEventHandler
{
    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractCreated,
        ContractPaymentEventTypes.ContractAccepted,
        ContractPaymentEventTypes.ContractActivated,
        ContractPaymentEventTypes.ContractCompleted,
        ContractPaymentEventTypes.ContractTerminated,
        ContractPaymentEventTypes.MilestoneReadyForFunding,
        ContractPaymentEventTypes.MilestoneFundingStarted,
        ContractPaymentEventTypes.MilestoneFunded,
        ContractPaymentEventTypes.MilestoneFundingFailed,
        ContractPaymentEventTypes.MilestoneSubmitted,
        ContractPaymentEventTypes.MilestoneAutoAccepted,
        ContractPaymentEventTypes.MilestoneAccepted,
        ContractPaymentEventTypes.MilestoneChangesRequested,
        ContractPaymentEventTypes.MilestoneChangeRequestCreated,
        ContractPaymentEventTypes.MilestoneChangeRequestApproved,
        ContractPaymentEventTypes.MilestoneChangeRequestRejected,
        ContractPaymentEventTypes.MilestoneChangeRequestCancelled,
        ContractPaymentEventTypes.FundsReleased,
        ContractPaymentEventTypes.FundsRefunded,
        ContractPaymentEventTypes.DisputeOpened,
        ContractPaymentEventTypes.DisputeAssigned,
        ContractPaymentEventTypes.DisputeResolved,
        ContractPaymentEventTypes.DisputeClosed
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var context = await new ContractIntegrationEventResolver(dbContext)
            .ResolveAsync(message, cancellationToken);
        await conversationService.AppendSystemMessageAsync(
            new ContractConversationSystemMessage(
                message.Id,
                context.ProposalId,
                MapType(message.EventType),
                context.RelatedEntityId,
                message.CreatedAt),
            cancellationToken);
    }

    private static ContractConversationMessageType MapType(string eventType)
    {
        return eventType switch
        {
            ContractPaymentEventTypes.ContractCreated =>
                ContractConversationMessageType.ContractCreated,
            ContractPaymentEventTypes.ContractAccepted =>
                ContractConversationMessageType.ContractAccepted,
            ContractPaymentEventTypes.ContractActivated =>
                ContractConversationMessageType.ContractActivated,
            ContractPaymentEventTypes.ContractCompleted =>
                ContractConversationMessageType.ContractCompleted,
            ContractPaymentEventTypes.ContractTerminated =>
                ContractConversationMessageType.ContractTerminated,
            ContractPaymentEventTypes.MilestoneReadyForFunding =>
                ContractConversationMessageType.MilestoneReadyForFunding,
            ContractPaymentEventTypes.MilestoneFundingStarted =>
                ContractConversationMessageType.MilestoneFundingStarted,
            ContractPaymentEventTypes.MilestoneFunded =>
                ContractConversationMessageType.MilestoneFunded,
            ContractPaymentEventTypes.MilestoneFundingFailed =>
                ContractConversationMessageType.MilestoneFundingFailed,
            ContractPaymentEventTypes.MilestoneSubmitted =>
                ContractConversationMessageType.MilestoneSubmitted,
            ContractPaymentEventTypes.MilestoneAutoAccepted =>
                ContractConversationMessageType.MilestoneAutoAccepted,
            ContractPaymentEventTypes.MilestoneAccepted =>
                ContractConversationMessageType.MilestoneAccepted,
            ContractPaymentEventTypes.MilestoneChangesRequested
                or ContractPaymentEventTypes.MilestoneChangeRequestCreated =>
                ContractConversationMessageType.MilestoneChangesRequested,
            ContractPaymentEventTypes.MilestoneChangeRequestApproved =>
                ContractConversationMessageType.MilestoneChangeRequestApproved,
            ContractPaymentEventTypes.MilestoneChangeRequestRejected =>
                ContractConversationMessageType.MilestoneChangeRequestRejected,
            ContractPaymentEventTypes.MilestoneChangeRequestCancelled =>
                ContractConversationMessageType.MilestoneChangeRequestCancelled,
            ContractPaymentEventTypes.FundsReleased =>
                ContractConversationMessageType.FundsReleased,
            ContractPaymentEventTypes.FundsRefunded =>
                ContractConversationMessageType.FundsRefunded,
            ContractPaymentEventTypes.DisputeOpened =>
                ContractConversationMessageType.DisputeOpened,
            ContractPaymentEventTypes.DisputeAssigned =>
                ContractConversationMessageType.DisputeAssigned,
            ContractPaymentEventTypes.DisputeResolved =>
                ContractConversationMessageType.DisputeResolved,
            ContractPaymentEventTypes.DisputeClosed =>
                ContractConversationMessageType.DisputeClosed,
            _ => throw new InvalidOperationException(
                "نوع حدث المحادثة التعاقدية غير مدعوم.")
        };
    }
}
