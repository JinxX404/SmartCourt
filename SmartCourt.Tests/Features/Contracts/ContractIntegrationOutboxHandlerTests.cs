using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Chat.Integration;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Events;
using SmartCourt.Features.Contracts.Integration;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Disputes.Enums;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Notifications.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Contracts;

public sealed class ContractIntegrationOutboxHandlerTests
{
    private static readonly DateTime UtcNow =
        new(2026, 8, 1, 10, 0, 0, DateTimeKind.Utc);

    [Fact]
    public async Task ConversationHandler_MapsFundsReleaseToMilestone()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        await context.SaveChangesAsync();
        var conversation = new RecordingConversationService();
        var handler = new ContractConversationIntegrationOutboxHandler(
            context,
            conversation);
        var eventId = Guid.NewGuid();
        var message = CreateMessage(
            eventId,
            ContractPaymentEventTypes.FundsReleased,
            "EscrowHold",
            Guid.NewGuid(),
            new FundsReleasedEventPayload(
                state.Milestone.Id,
                Guid.NewGuid(),
                Guid.NewGuid(),
                950m,
                50m));

        await handler.HandleAsync(message, CancellationToken.None);

        var delivered = Assert.Single(conversation.Messages);
        Assert.Equal(eventId, delivered.EventId);
        Assert.Equal(state.Contract.ProposalId, delivered.ProposalId);
        Assert.Equal(
            ContractConversationMessageType.FundsReleased,
            delivered.Type);
        Assert.Equal(state.Milestone.Id, delivered.RelatedEntityId);
        Assert.Contains(
            ContractPaymentEventTypes.DisputeResolved,
            handler.EventTypes);
        Assert.Contains(
            ContractPaymentEventTypes.MilestoneFundingFailed,
            handler.EventTypes);
    }

    [Fact]
    public async Task ConversationHandler_MapsSubmissionChangesToMilestone()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        await context.SaveChangesAsync();
        var conversation = new RecordingConversationService();
        var handler = new ContractConversationIntegrationOutboxHandler(
            context,
            conversation);
        var eventId = Guid.NewGuid();
        var message = CreateMessage(
            eventId,
            ContractPaymentEventTypes.MilestoneChangesRequested,
            "Milestone",
            state.Milestone.Id,
            new ContractPaymentAggregateEventPayload(state.Milestone.Id));

        await handler.HandleAsync(message, CancellationToken.None);

        var delivered = Assert.Single(conversation.Messages);
        Assert.Equal(eventId, delivered.EventId);
        Assert.Equal(state.Contract.ProposalId, delivered.ProposalId);
        Assert.Equal(
            ContractConversationMessageType.MilestoneChangesRequested,
            delivered.Type);
        Assert.Equal(state.Milestone.Id, delivered.RelatedEntityId);
    }

    [Fact]
    public async Task NotificationHandler_SendsDisputeAssignmentToAllRecipients()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        var moderatorUserId = Guid.NewGuid();
        var dispute = new Dispute(
            Guid.NewGuid(),
            state.Contract.Id,
            state.Milestone.Id,
            state.Contract.ClientUserId,
            DisputeCategory.DeliverableQuality,
            "نزاع على التسليم",
            "وصف واضح للنزاع القائم على المرحلة.",
            DisputeRequestedOutcome.Refund,
            UtcNow)
        {
            AssignedModeratorUserId = moderatorUserId,
            Status = DisputeStatus.Assigned
        };
        context.Add(dispute);
        await context.SaveChangesAsync();
        var notifications = new RecordingNotificationService();
        var handler = new ContractNotificationOutboxHandler(
            context,
            [notifications]);
        var eventId = Guid.NewGuid();
        var message = CreateMessage(
            eventId,
            ContractPaymentEventTypes.DisputeAssigned,
            "Dispute",
            dispute.Id,
            new ContractPaymentAggregateEventPayload(dispute.Id));

        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(3, notifications.Notifications.Count);
        Assert.Equal(
            new[]
            {
                state.Contract.ClientUserId,
                state.Contract.LawyerUserId,
                moderatorUserId
            }.Order(),
            notifications.Notifications
                .Select(item => item.RecipientUserId)
                .Order());
        Assert.All(
            notifications.Notifications,
            notification =>
            {
                Assert.Equal(eventId, notification.EventId);
                Assert.Equal(
                    ContractNotificationType.DisputeAssigned,
                    notification.Type);
                Assert.Equal("Dispute", notification.RelatedEntityType);
                Assert.Equal(dispute.Id, notification.RelatedEntityId);
            });
    }

    [Fact]
    public async Task NotificationHandler_MissingOwnerServiceFailsForRetry()
    {
        await using var context = CreateContext();
        var handler = new ContractNotificationOutboxHandler(context, []);
        var message = CreateMessage(
            Guid.NewGuid(),
            ContractPaymentEventTypes.ContractCreated,
            "Contract",
            Guid.NewGuid(),
            new ContractPaymentAggregateEventPayload(Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(message, CancellationToken.None));

        Assert.Contains("سيعاد إرسال الحدث تلقائيًا", exception.Message);
    }

    [Fact]
    public async Task CaseLifecycleHandler_ForwardsCompletionWithoutCaseMutation()
    {
        await using var context = CreateContext();
        var state = AddContractAndMilestone(context);
        await context.SaveChangesAsync();
        var caseLifecycle = new RecordingCaseLifecycleService();
        var handler = new ContractCaseLifecycleOutboxHandler(
            context,
            [caseLifecycle]);
        var eventId = Guid.NewGuid();
        var message = CreateMessage(
            eventId,
            ContractPaymentEventTypes.ContractCompleted,
            "Contract",
            state.Contract.Id,
            new ContractPaymentAggregateEventPayload(state.Contract.Id));

        await handler.HandleAsync(message, CancellationToken.None);

        var delivered = Assert.IsType<ContractCaseLifecycleUpdate>(
            caseLifecycle.Update);
        Assert.Equal(eventId, delivered.EventId);
        Assert.Equal(state.Contract.LegalCaseId, delivered.LegalCaseId);
        Assert.Equal(state.Contract.Id, delivered.ContractId);
        Assert.Equal(
            ContractCaseLifecycleTransition.ContractCompleted,
            delivered.Transition);
    }

    private static TestState AddContractAndMilestone(
        ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد اختبار التكامل",
            "شروط عقد صالحة لاختبار رسائل التكامل.",
            UtcNow);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة اختبار التكامل",
            null,
            1,
            1_000m,
            14,
            null,
            UtcNow);
        context.AddRange(contract, milestone);
        return new TestState(contract, milestone);
    }

    private static OutboxMessage CreateMessage<T>(
        Guid id,
        string eventType,
        string aggregateType,
        Guid aggregateId,
        T payload)
    {
        return new OutboxMessage(
            id,
            eventType,
            1,
            JsonSerializer.Serialize(payload),
            aggregateType,
            aggregateId,
            Guid.NewGuid(),
            UtcNow,
            UtcNow);
    }

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(
                    $"contract-integrations-{Guid.NewGuid():N}")
                .Options,
            new FixedTimeProvider());
    }

    private sealed record TestState(
        Contract Contract,
        Milestone Milestone);

    private sealed class RecordingConversationService
        : IContractConversationService
    {
        public List<ContractConversationSystemMessage> Messages { get; } = [];

        public Task AppendSystemMessageAsync(
            ContractConversationSystemMessage message,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingNotificationService
        : IContractNotificationService
    {
        public List<ContractNotification> Notifications { get; } = [];

        public Task PublishAsync(
            ContractNotification notification,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Notifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingCaseLifecycleService
        : IContractCaseLifecycleService
    {
        public ContractCaseLifecycleUpdate? Update { get; private set; }

        public Task ApplyAsync(
            ContractCaseLifecycleUpdate update,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Update = update;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(UtcNow);
    }
}
