using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Events;

internal sealed class ContractIntegrationEventResolver(
    ApplicationDbContext dbContext)
{
    private static readonly JsonSerializerOptions SerializerOptions =
        new()
        {
            PropertyNameCaseInsensitive = true
        };

    public async Task<ContractIntegrationEventContext> ResolveAsync(
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
                    message.AggregateId,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneSubmitted =>
                await ResolveMilestoneAsync(
                    Deserialize<MilestoneSubmissionEventPayload>(message)
                        .MilestoneId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneAutoAccepted =>
                await ResolveMilestoneAsync(
                    Deserialize<MilestoneAutoAcceptedEventPayload>(message)
                        .MilestoneId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneAccepted =>
                await ResolveMilestoneAsync(
                    Deserialize<MilestoneAcceptanceEventPayload>(message)
                        .MilestoneId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneChangesRequested =>
                await ResolveMilestoneAsync(
                    message.AggregateId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.MilestoneChangeRequestCreated
                or ContractPaymentEventTypes.MilestoneChangeRequestApproved
                or ContractPaymentEventTypes.MilestoneChangeRequestRejected
                or ContractPaymentEventTypes.MilestoneChangeRequestCancelled =>
                await ResolveChangeRequestAsync(message, cancellationToken),
            ContractPaymentEventTypes.FundsReleased =>
                await ResolveMilestoneAsync(
                    Deserialize<FundsReleasedEventPayload>(message).MilestoneId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.FundsRefunded =>
                await ResolveMilestoneAsync(
                    Deserialize<FundsRefundedEventPayload>(message).MilestoneId,
                    "Milestone",
                    null,
                    cancellationToken),
            ContractPaymentEventTypes.DisputeOpened
                or ContractPaymentEventTypes.DisputeAssigned
                or ContractPaymentEventTypes.DisputeResolved
                or ContractPaymentEventTypes.DisputeClosed =>
                await ResolveDisputeAsync(
                    message.AggregateId,
                    cancellationToken),
            _ => await ResolveMilestoneAsync(
                message.AggregateId,
                "Milestone",
                null,
                cancellationToken)
        };
    }

    private async Task<ContractIntegrationEventContext> ResolveContractAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await dbContext.Contracts
            .AsNoTracking()
            .Where(contract => contract.Id == contractId)
            .Select(contract => new ContractIntegrationEventContext(
                contract.ProposalId,
                contract.Id,
                contract.LegalCaseId,
                contract.ClientUserId,
                contract.LawyerUserId,
                null,
                "Contract",
                contract.Id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إرسال حدث التكامل لأن العقد المرتبط به غير موجود.");
    }

    private async Task<ContractIntegrationEventContext> ResolveMilestoneAsync(
        Guid milestoneId,
        string relatedEntityType,
        Guid? relatedEntityId,
        CancellationToken cancellationToken)
    {
        return await (
            from milestone in dbContext.Milestones.AsNoTracking()
            join contract in dbContext.Contracts.AsNoTracking()
                on milestone.ContractId equals contract.Id
            where milestone.Id == milestoneId
            select new ContractIntegrationEventContext(
                contract.ProposalId,
                contract.Id,
                contract.LegalCaseId,
                contract.ClientUserId,
                contract.LawyerUserId,
                null,
                relatedEntityType,
                relatedEntityId ?? milestone.Id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إرسال حدث التكامل لأن المرحلة المرتبطة به غير موجودة.");
    }

    private async Task<ContractIntegrationEventContext>
        ResolveChangeRequestAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
    {
        var payload = Deserialize<MilestoneChangeRequestEventPayload>(message);
        return await ResolveMilestoneAsync(
            payload.MilestoneId,
            "MilestoneChangeRequest",
            payload.ChangeRequestId,
            cancellationToken);
    }

    private async Task<ContractIntegrationEventContext> ResolveDisputeAsync(
        Guid disputeId,
        CancellationToken cancellationToken)
    {
        return await (
            from dispute in dbContext.Disputes.AsNoTracking()
            join contract in dbContext.Contracts.AsNoTracking()
                on dispute.ContractId equals contract.Id
            where dispute.Id == disputeId
            select new ContractIntegrationEventContext(
                contract.ProposalId,
                contract.Id,
                contract.LegalCaseId,
                contract.ClientUserId,
                contract.LawyerUserId,
                dispute.AssignedModeratorUserId,
                "Dispute",
                dispute.Id))
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException(
                "تعذر إرسال حدث التكامل لأن النزاع المرتبط به غير موجود.");
    }

    private static T Deserialize<T>(OutboxMessage message)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(
                    message.Payload,
                    SerializerOptions)
                ?? throw new JsonException();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "تعذر إرسال حدث التكامل لأن بياناته غير صالحة.",
                exception);
        }
    }
}

internal sealed record ContractIntegrationEventContext(
    Guid ProposalId,
    Guid ContractId,
    Guid LegalCaseId,
    Guid ClientUserId,
    Guid LawyerUserId,
    Guid? ModeratorUserId,
    string RelatedEntityType,
    Guid RelatedEntityId);
