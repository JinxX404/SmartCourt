using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class NotificationOutboxHandlerTests
{
    [Theory]
    [InlineData(
        ContractPaymentEventTypes.ProposalCreated,
        "proposal.created",
        "Information",
        "عرض جديد",
        "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
        true)]
    [InlineData(
        ContractPaymentEventTypes.ProposalAccepted,
        "proposal.accepted",
        "Success",
        "تم قبول العرض",
        "وافق المحامي على عرضك.",
        false)]
    [InlineData(
        ContractPaymentEventTypes.ProposalRejected,
        "proposal.rejected",
        "Warning",
        "تم رفض العرض",
        "رفض المحامي عرضك. يمكنك مراجعة التفاصيل واختيار محامٍ آخر.",
        false)]
    [InlineData(
        ContractPaymentEventTypes.ProposalCancelled,
        "proposal.cancelled",
        "Information",
        "تم إلغاء العرض",
        "ألغى الموكل العرض المعلق.",
        true)]
    [InlineData(
        ContractPaymentEventTypes.ProposalExpired,
        "proposal.expired",
        "Warning",
        "انتهت صلاحية العرض",
        "لم يرد المحامي على العرض خلال ثلاثة أيام.",
        false)]
    [InlineData(
        ContractPaymentEventTypes.ProposalSuperseded,
        "proposal.superseded",
        "Information",
        "تم إغلاق العرض",
        "نعتذر، تم إسناد القضية إلى محامٍ آخر ولم تعد محادثة التفاوض متاحة حفاظًا على خصوصية الموكل.",
        true)]
    public async Task HandleAsync_MapsAndPersistsArabicProposalNotification(
        string eventType,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody,
        bool recipientIsLawyer)
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var handler = CreateProposalHandler(context, notifier);
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
        Assert.Equal(expectedTitle, saved.Title);
        Assert.Equal(expectedBody, saved.Body);
        Assert.Equal(message.Id, saved.SourceEventId);
        Assert.Equal($"/proposals/{proposalId}", saved.ActionUrl);
        var pushed = Assert.Single(notifier.Created);
        Assert.Equal(expectedTitle, pushed.Notification.Title);
        Assert.Equal(proposalId.ToString(), pushed.Notification.Data!["proposalId"]);
        Assert.Equal(legalCaseId.ToString(), pushed.Notification.Data["legalCaseId"]);
    }

    [Fact]
    public async Task HandleAsync_TerminatedProposalNotifiesOtherParticipant()
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

        await CreateProposalHandler(context, notifier)
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(lawyerId, saved.RecipientUserId);
        Assert.Equal("proposal.terminated", saved.Type);
        Assert.Equal("انتهت المفاوضات", saved.Title);
    }

    [Fact]
    public async Task HandleAsync_RetryDoesNotInsertDuplicate()
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var handler = CreateProposalHandler(context, notifier);
        var proposalId = Guid.NewGuid();
        var message = CreateProposalMessage(proposalId);

        await handler.HandleAsync(message, CancellationToken.None);
        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(1, await context.Notifications.CountAsync());
        Assert.Equal(2, notifier.Created.Count);
        Assert.Equal(
            notifier.Created[0].Notification.Id,
            notifier.Created[1].Notification.Id);
    }

    [Fact]
    public async Task HandleAsync_AggregateMismatchFailsDeterministically()
    {
        await using var context = CreateContext();
        var handler = CreateProposalHandler(context, new RecordingNotifier());
        var message = CreateMessage(
            ContractPaymentEventTypes.ProposalCreated,
            Guid.NewGuid(),
            new ProposalEventPayload(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        Assert.Contains("do not match", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedVersionFailsDeterministically()
    {
        await using var context = CreateContext();
        var handler = CreateProposalHandler(context, new RecordingNotifier());
        var proposalId = Guid.NewGuid();
        var message = CreateProposalMessage(proposalId, eventVersion: 2);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        Assert.Contains("version 2 is unsupported", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_InvalidPayloadFailsDeterministically()
    {
        await using var context = CreateContext();
        var handler = CreateProposalHandler(context, new RecordingNotifier());
        var proposalId = Guid.NewGuid();
        var message = CreateRawMessage(
            ContractPaymentEventTypes.ProposalCreated,
            proposalId,
            "{");

        await Assert.ThrowsAsync<JsonException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_UnregisteredEventTypeFailsDeterministically()
    {
        await using var context = CreateContext();
        var handler = CreateProposalHandler(context, new RecordingNotifier());
        var message = CreateMessage(
            "test.unregistered",
            Guid.NewGuid(),
            new { value = "test" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        Assert.Contains("No notification mapper is registered", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_MapperMayReturnNoNotifications()
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var handler = new NotificationOutboxHandler(
            context,
            notifier,
            [new StubMapper("test.none", [])]);
        var message = CreateMessage(
            "test.none",
            Guid.NewGuid(),
            new { value = "test" });

        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Empty(context.Notifications);
        Assert.Empty(notifier.Created);
    }

    [Fact]
    public async Task HandleAsync_GenericMapperCanCreateMultipleNotifications()
    {
        await using var context = CreateContext();
        var notifier = new RecordingNotifier();
        var firstRecipient = Guid.NewGuid();
        var secondRecipient = Guid.NewGuid();
        var mapper = new StubMapper(
            "test.multiple",
            [
                Draft(firstRecipient, "test.first"),
                Draft(secondRecipient, "test.second")
            ]);
        var handler = new NotificationOutboxHandler(context, notifier, [mapper]);
        var message = CreateMessage(
            "test.multiple",
            Guid.NewGuid(),
            new { value = "test" });

        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(2, await context.Notifications.CountAsync());
        Assert.Equal(2, notifier.Created.Count);
        Assert.Contains(notifier.Created, item => item.UserId == firstRecipient);
        Assert.Contains(notifier.Created, item => item.UserId == secondRecipient);
    }

    [Fact]
    public void Constructor_DuplicateEventOwnershipFailsAtStartup()
    {
        using var context = CreateContext();
        var first = new StubMapper("test.duplicate", []);
        var second = new StubMapper("test.duplicate", []);

        var exception = Assert.Throws<InvalidOperationException>(() =>
            new NotificationOutboxHandler(
                context,
                new RecordingNotifier(),
                [first, second]));

        Assert.Contains("Multiple notification mappers", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_DuplicateDraftIdentityFailsBeforePersistence()
    {
        await using var context = CreateContext();
        var recipientId = Guid.NewGuid();
        var mapper = new StubMapper(
            "test.duplicate-drafts",
            [
                Draft(recipientId, "test.same"),
                Draft(recipientId, "test.same")
            ]);
        var handler = new NotificationOutboxHandler(
            context,
            new RecordingNotifier(),
            [mapper]);
        var message = CreateMessage(
            "test.duplicate-drafts",
            Guid.NewGuid(),
            new { value = "test" });

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        Assert.Contains("duplicate recipient and type", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_InvalidLaterDraftLeavesNoPartialInsertTracked()
    {
        await using var context = CreateContext();
        var mapper = new StubMapper(
            "test.invalid-later-draft",
            [
                Draft(Guid.NewGuid(), "test.valid"),
                Draft(Guid.NewGuid(), string.Empty)
            ]);
        var handler = new NotificationOutboxHandler(
            context,
            new RecordingNotifier(),
            [mapper]);
        var message = CreateMessage(
            "test.invalid-later-draft",
            Guid.NewGuid(),
            new { value = "test" });

        await Assert.ThrowsAsync<BusinessException>(() =>
            handler.HandleAsync(message, CancellationToken.None));

        await context.SaveChangesAsync();
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_RealtimeFailureThenRetryKeepsSinglePersistedRow()
    {
        await using var context = CreateContext();
        var failingNotifier = new RecordingNotifier(failFirstCreate: true);
        var handler = CreateProposalHandler(context, failingNotifier);
        var proposalId = Guid.NewGuid();
        var message = CreateProposalMessage(proposalId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(message, CancellationToken.None));
        await handler.HandleAsync(message, CancellationToken.None);

        Assert.Equal(1, await context.Notifications.CountAsync());
        Assert.Single(failingNotifier.Created);
    }

    private static NotificationOutboxHandler CreateProposalHandler(
        ApplicationDbContext context,
        INotificationRealtimeNotifier notifier) => new(
            context,
            notifier,
            [new ProposalNotificationEventMapper()]);

    private static NotificationDraft Draft(Guid recipientId, string type) => new(
        recipientId,
        type,
        NotificationSeverity.Information,
        "عنوان",
        "نص الإشعار",
        null,
        new Dictionary<string, string> { ["source"] = "test" });

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ApplicationDbContext(options);
    }

    private static OutboxMessage CreateProposalMessage(
        Guid proposalId,
        int eventVersion = 1) => CreateMessage(
            ContractPaymentEventTypes.ProposalCreated,
            proposalId,
            new ProposalEventPayload(
                proposalId,
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()),
            eventVersion);

    private static OutboxMessage CreateMessage<T>(
        string eventType,
        Guid aggregateId,
        T payload,
        int eventVersion = 1)
    {
        return CreateRawMessage(
            eventType,
            aggregateId,
            JsonSerializer.Serialize(payload),
            eventVersion);
    }

    private static OutboxMessage CreateRawMessage(
        string eventType,
        Guid aggregateId,
        string payload,
        int eventVersion = 1)
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
            eventVersion,
            payload,
            "TestAggregate",
            aggregateId,
            Guid.NewGuid(),
            now,
            now);
    }

    private sealed class StubMapper(
        string eventType,
        IReadOnlyCollection<NotificationDraft> drafts)
        : INotificationEventMapper
    {
        public IReadOnlyCollection<string> EventTypes => [eventType];

        public Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
            OutboxMessage message,
            CancellationToken cancellationToken) => Task.FromResult(drafts);
    }

    private sealed class RecordingNotifier(bool failFirstCreate = false)
        : INotificationRealtimeNotifier
    {
        private bool _shouldFail = failFirstCreate;

        public List<(Guid UserId, NotificationDto Notification)> Created { get; } = [];

        public Task NotificationCreatedAsync(
            Guid userId,
            NotificationDto notification,
            CancellationToken cancellationToken)
        {
            if (_shouldFail)
            {
                _shouldFail = false;
                throw new InvalidOperationException("Simulated realtime failure.");
            }

            Created.Add((userId, notification));
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
