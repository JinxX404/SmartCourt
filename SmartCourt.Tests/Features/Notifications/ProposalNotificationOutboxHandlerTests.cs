using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class ProposalNotificationOutboxHandlerTests
{
    [Theory]
    [InlineData(ContractPaymentEventTypes.ProposalCreated, "proposal.created", "Information", true)]
    [InlineData(ContractPaymentEventTypes.ProposalAccepted, "proposal.accepted", "Success", false)]
    [InlineData(ContractPaymentEventTypes.ProposalRejected, "proposal.rejected", "Warning", false)]
    [InlineData(ContractPaymentEventTypes.ProposalCancelled, "proposal.cancelled", "Information", true)]
    [InlineData(ContractPaymentEventTypes.ProposalExpired, "proposal.expired", "Warning", false)]
    [InlineData(ContractPaymentEventTypes.ProposalSuperseded, "proposal.superseded", "Information", true)]
    public async Task HandleAsync_MapsAndPersistsProposalNotification(
        string eventType,
        string expectedType,
        string expectedSeverity,
        bool recipientIsLawyer)
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var handler = new ProposalNotificationOutboxHandler(context, notifier);
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var legalCaseId = Guid.NewGuid();
        var message = CreateMessage(
            eventType,
            proposalId,
            new ProposalEventPayload(
                proposalId,
                legalCaseId,
                clientId,
                lawyerId));

        await handler.HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(recipientIsLawyer ? lawyerId : clientId, saved.RecipientUserId);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal(expectedSeverity, saved.Severity.ToString());
        Assert.Equal(message.Id, saved.SourceEventId);
        Assert.Equal($"/proposals/{proposalId}", saved.ActionUrl);
        Assert.Single(notifier.Created);
    }

    [Fact]
    public async Task HandleAsync_TerminationNotifiesOtherParticipant()
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var clientId = Guid.NewGuid();
        var lawyerId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var message = CreateMessage(
            ContractPaymentEventTypes.ProposalTerminated,
            proposalId,
            new ProposalEventPayload(
                proposalId,
                Guid.NewGuid(),
                clientId,
                lawyerId,
                clientId,
                "No agreement"));

        await new ProposalNotificationOutboxHandler(context, notifier)
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(lawyerId, saved.RecipientUserId);
        Assert.Equal("proposal.terminated", saved.Type);
    }

    [Fact]
    public async Task HandleAsync_SupersededProposalExplainsPrivacyClosureToLawyer()
    {
        await using var context = CreateContext();
        var lawyerId = Guid.NewGuid();
        var proposalId = Guid.NewGuid();
        var message = CreateMessage(
            ContractPaymentEventTypes.ProposalSuperseded,
            proposalId,
            new ProposalEventPayload(
                proposalId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                lawyerId));

        await new ProposalNotificationOutboxHandler(
                context,
                new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(lawyerId, saved.RecipientUserId);
        Assert.Contains("sorry", saved.Body, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "assigned to another lawyer",
            saved.Body,
            StringComparison.OrdinalIgnoreCase);
        Assert.Contains(
            "no longer available",
            saved.Body,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task HandleAsync_RetryDoesNotInsertDuplicate()
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var handler = new ProposalNotificationOutboxHandler(context, notifier);
        var proposalId = Guid.NewGuid();
        var message = CreateMessage(
            ContractPaymentEventTypes.ProposalCreated,
            proposalId,
            new ProposalEventPayload(
                proposalId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        await handler.HandleAsync(message, CancellationToken.None);
        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(1, await context.Notifications.CountAsync());
        Assert.Equal(2, notifier.Created.Count);
    }

    [Fact]
    public async Task HandleAsync_AggregateMismatchFailsDeterministically()
    {
        await using var context = CreateContext();
        var handler = new ProposalNotificationOutboxHandler(
            context,
            new RecordingNotifier());
        var message = CreateMessage(
            ContractPaymentEventTypes.ProposalCreated,
            Guid.NewGuid(),
            new ProposalEventPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static OutboxMessage CreateMessage<T>(
        string eventType,
        Guid aggregateId,
        T payload)
    {
        var now = new DateTime(
            2026,
            8,
            9,
            10,
            0,
            0,
            DateTimeKind.Utc);
        return new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            1,
            JsonSerializer.Serialize(payload),
            "Proposal",
            aggregateId,
            Guid.NewGuid(),
            now,
            now);
    }

    private sealed class RecordingNotifier : INotificationRealtimeNotifier
    {
        public List<NotificationDto> Created { get; } = [];

        public Task NotificationCreatedAsync(
            Guid userId,
            NotificationDto notification,
            CancellationToken cancellationToken)
        {
            Created.Add(notification);
            return Task.CompletedTask;
        }

        public Task NotificationReadAsync(
            Guid userId,
            NotificationReadDto update,
            CancellationToken cancellationToken) => Task.CompletedTask;

        public Task NotificationsReadAllAsync(
            Guid userId,
            NotificationsReadAllDto update,
            CancellationToken cancellationToken) => Task.CompletedTask;
    }
}
