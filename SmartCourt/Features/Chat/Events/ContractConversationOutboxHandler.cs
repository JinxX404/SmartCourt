using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Events;

public sealed class ContractConversationOutboxHandler(
    ApplicationDbContext context,
    IContractConversationService conversationService)
    : IOutboxEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractCreated,
        ContractPaymentEventTypes.ContractAccepted,
        ContractPaymentEventTypes.ContractActivated,
        ContractPaymentEventTypes.ContractCompleted,
        ContractPaymentEventTypes.ContractTerminated,
        ContractPaymentEventTypes.MilestoneReadyForFunding,
        ContractPaymentEventTypes.MilestoneSubmitted,
        ContractPaymentEventTypes.MilestoneAccepted,
        ContractPaymentEventTypes.MilestoneChangesRequested,
        ContractPaymentEventTypes.MilestoneChangeRequestApproved,
        ContractPaymentEventTypes.MilestoneChangeRequestRejected,
        ContractPaymentEventTypes.MilestoneChangeRequestCancelled
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (!TryMapType(
                message.EventType,
                out var conversationMessageType))
        {
            return;
        }

        var related = await ResolveRelatedEntityAsync(
            message,
            cancellationToken);
        if (related is null)
        {
            return;
        }

        await conversationService.AppendSystemMessageAsync(
            new ContractConversationSystemMessage(
                message.Id,
                related.ProposalId,
                conversationMessageType,
                related.RelatedEntityId,
                message.CreatedAt),
            cancellationToken);
    }

    private async Task<RelatedConversationEntity?> ResolveRelatedEntityAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        return message.EventType switch
        {
            ContractPaymentEventTypes.ContractCreated
                or ContractPaymentEventTypes.ContractAccepted
                or ContractPaymentEventTypes.ContractActivated
                or ContractPaymentEventTypes.ContractCompleted
                or ContractPaymentEventTypes.ContractTerminated =>
                await ResolveContractAsync(
                    Deserialize<ContractPaymentAggregateEventPayload>(message)
                        .EntityId,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneReadyForFunding =>
                await ResolveMilestoneAsync(
                    Deserialize<ContractPaymentAggregateEventPayload>(message)
                        .EntityId,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneSubmitted =>
                await ResolveMilestoneAsync(
                    Deserialize<MilestoneSubmissionEventPayload>(message)
                        .MilestoneId,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneAccepted =>
                await ResolveMilestoneAsync(
                    Deserialize<MilestoneAcceptanceEventPayload>(message)
                        .MilestoneId,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneChangesRequested
                or ContractPaymentEventTypes.MilestoneChangeRequestApproved
                or ContractPaymentEventTypes.MilestoneChangeRequestRejected
                or ContractPaymentEventTypes.MilestoneChangeRequestCancelled =>
                await ResolveChangeRequestAsync(message, cancellationToken),
            _ => null
        };
    }

    private async Task<RelatedConversationEntity?> ResolveContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await context.Contracts
            .AsNoTracking()
            .Where(contract => contract.Id == contractId)
            .Select(contract => new RelatedConversationEntity(
                contract.ProposalId,
                contract.Id))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<RelatedConversationEntity?> ResolveMilestoneAsync(
        Guid milestoneId,
        CancellationToken cancellationToken)
    {
        return await (
            from milestone in context.Milestones.AsNoTracking()
            join contract in context.Contracts.AsNoTracking()
                on milestone.ContractId equals contract.Id
            where milestone.Id == milestoneId
            select new RelatedConversationEntity(
                contract.ProposalId,
                milestone.Id))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private async Task<RelatedConversationEntity?> ResolveChangeRequestAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<MilestoneChangeRequestEventPayload>(message);
        return await (
            from milestone in context.Milestones.AsNoTracking()
            join contract in context.Contracts.AsNoTracking()
                on milestone.ContractId equals contract.Id
            where milestone.Id == payload.MilestoneId
            select new RelatedConversationEntity(
                contract.ProposalId,
                payload.ChangeRequestId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static bool TryMapType(
        string eventType,
        out ContractConversationMessageType type)
    {
        type = eventType switch
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
            ContractPaymentEventTypes.MilestoneChangesRequested =>
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
            _ => default
        };
        return type != default;
    }

    private static T Deserialize<T>(OutboxMessage message)
    {
        return JsonSerializer.Deserialize<T>(
                message.Payload,
                SerializerOptions)
            ?? throw new InvalidOperationException(
                $"Outbox payload for {message.EventType} is invalid.");
    }

    private sealed record RelatedConversationEntity(
        Guid ProposalId,
        Guid RelatedEntityId);
}
