using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Contracts.Events;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class ContractNotificationEventMapperTests
{
    private static readonly DateTime UtcNow = new(
        2026,
        8,
        9,
        12,
        0,
        0,
        DateTimeKind.Utc);

    [Theory]
    [InlineData(
        ContractPaymentEventTypes.ContractCreated,
        "contract.created",
        "Information",
        "مسودة عقد جديدة",
        "أنشأ المحامي مسودة عقد جديدة لتراجع شروطها.")]
    [InlineData(
        ContractPaymentEventTypes.ContractDraftUpdated,
        "contract.draft-updated",
        "Warning",
        "تم تحديث مسودة العقد",
        "تم تحديث شروط العقد، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك.")]
    public async Task HandleAsync_ClientOnlyContractEventPersistsArabicContract(
        string eventType,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var notifier = new RecordingNotifier();
        var handler = CreateHandler(context, notifier);
        var payload = eventType == ContractPaymentEventTypes.ContractCreated
            ? (object)new ContractPaymentAggregateEventPayload(contract.Id)
            : new ContractDraftUpdatedEventPayload(contract.Id);
        var message = CreateMessage(eventType, 1, contract.Id, payload);

        await handler.HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(contract.ClientUserId, saved.RecipientUserId);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal(expectedSeverity, saved.Severity.ToString());
        Assert.Equal(expectedTitle, saved.Title);
        Assert.Equal(expectedBody, saved.Body);
        Assert.Equal($"/dashboard/contracts/{contract.Id}", saved.ActionUrl);
        AssertMetadata(Assert.Single(notifier.Created).Notification, contract);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task HandleAsync_FirstAcceptanceNotifiesOnlyCounterparty(
        bool clientAccepts)
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var actorUserId = clientAccepts
            ? contract.ClientUserId
            : contract.LawyerUserId;
        var expectedRecipient = clientAccepts
            ? contract.LawyerUserId
            : contract.ClientUserId;
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractAccepted,
            2,
            contract.Id,
            new ContractAcceptanceRecordedEventPayload(
                contract.Id,
                actorUserId,
                true));

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(expectedRecipient, saved.RecipientUserId);
        Assert.Equal("contract.acceptance-recorded", saved.Type);
        Assert.Equal("موافقة جديدة على العقد", saved.Title);
        Assert.Equal(
            "وافق الطرف الآخر على نسخة العقد الحالية، والعقد بانتظار موافقتك.",
            saved.Body);
    }

    [Fact]
    public async Task HandleAsync_FinalAcceptanceDoesNotCreateMisleadingNotification()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractAccepted,
            2,
            contract.Id,
            new ContractAcceptanceRecordedEventPayload(
                contract.Id,
                contract.LawyerUserId,
                false));

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_HistoricalAcceptanceV1IsSafeNoOp()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractAccepted,
            1,
            contract.Id,
            new ContractPaymentAggregateEventPayload(contract.Id));

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        Assert.Empty(context.Notifications);
    }

    [Theory]
    [InlineData(
        ContractPaymentEventTypes.ContractActivated,
        "contract.activated",
        "Success",
        "تم تفعيل العقد",
        "أصبح العقد نشطًا ويمكن بدء تنفيذ مراحله.")]
    [InlineData(
        ContractPaymentEventTypes.ContractCompleted,
        "contract.completed",
        "Success",
        "اكتمل العقد",
        "اكتملت جميع مراحل العقد وتسوياته بنجاح.")]
    [InlineData(
        ContractPaymentEventTypes.ContractTerminated,
        "contract.terminated",
        "Warning",
        "تم إنهاء العقد",
        "اكتملت إجراءات إنهاء العقد وتسويته.")]
    public async Task HandleAsync_LifecycleEventNotifiesBothParticipants(
        string eventType,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        object payload = eventType == ContractPaymentEventTypes.ContractTerminated
            ? new ContractTerminatedEventPayload(
                contract.Id,
                contract.LegalCaseId,
                contract.ClientUserId)
            : new ContractPaymentAggregateEventPayload(contract.Id);
        var message = CreateMessage(eventType, 1, contract.Id, payload);

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications
            .OrderBy(item => item.RecipientUserId)
            .ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Equal(
            new[] { contract.ClientUserId, contract.LawyerUserId }.Order(),
            saved.Select(item => item.RecipientUserId));
        Assert.All(saved, item =>
        {
            Assert.Equal(expectedType, item.Type);
            Assert.Equal(expectedSeverity, item.Severity.ToString());
            Assert.Equal(expectedTitle, item.Title);
            Assert.Equal(expectedBody, item.Body);
            Assert.Equal($"/dashboard/contracts/{contract.Id}", item.ActionUrl);
        });
    }

    [Fact]
    public async Task HandleAsync_PendingTerminationRequestNotifiesBothParticipants()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractTerminationRequested,
            1,
            contract.Id,
            new ContractTerminationRequestedEventPayload(
                contract.Id,
                contract.ClientUserId));

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Contains(saved, item =>
            item.RecipientUserId == contract.ClientUserId);
        Assert.Contains(saved, item =>
            item.RecipientUserId == contract.LawyerUserId);
        Assert.All(saved, item =>
        {
            Assert.Equal("contract.termination-requested", item.Type);
            Assert.Equal("تم طلب إنهاء العقد", item.Title);
            Assert.Equal(
                "تم تسجيل طلب إنهاء العقد، وتجري معالجة التسوية اللازمة.",
                item.Body);
        });
    }

    [Fact]
    public async Task HandleAsync_AlreadyTerminatedRequestNotifiesOnlyCounterparty()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        contract.Status = ContractStatus.Terminated;
        contract.TerminatedAt = UtcNow;
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractTerminationRequested,
            1,
            contract.Id,
            new ContractTerminationRequestedEventPayload(
                contract.Id,
                contract.ClientUserId));

        await CreateHandler(context, new RecordingNotifier())
            .HandleAsync(message, CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(contract.LawyerUserId, saved.RecipientUserId);
    }

    [Fact]
    public async Task HandleAsync_NonParticipantActorFailsBeforePersistence()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractAccepted,
            2,
            contract.Id,
            new ContractAcceptanceRecordedEventPayload(
                contract.Id,
                Guid.NewGuid(),
                true));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context, new RecordingNotifier())
                .HandleAsync(message, CancellationToken.None));

        Assert.Contains("not a contract participant", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_AggregateMismatchFailsBeforeContextLookup()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractCreated,
            1,
            Guid.NewGuid(),
            new ContractPaymentAggregateEventPayload(contract.Id));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context, new RecordingNotifier())
                .HandleAsync(message, CancellationToken.None));

        Assert.Contains("do not match", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedVersionFailsDeterministically()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractCreated,
            99,
            contract.Id,
            new ContractPaymentAggregateEventPayload(contract.Id));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context, new RecordingNotifier())
                .HandleAsync(message, CancellationToken.None));

        Assert.Contains("version 99 is unsupported", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_TerminationLegalCaseMismatchFails()
    {
        await using var context = CreateContext();
        var contract = AddContract(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.ContractTerminated,
            1,
            contract.Id,
            new ContractTerminatedEventPayload(
                contract.Id,
                Guid.NewGuid(),
                contract.ClientUserId));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context, new RecordingNotifier())
                .HandleAsync(message, CancellationToken.None));

        Assert.Contains("legal-case identifiers do not match", exception.Message);
        Assert.Empty(context.Notifications);
    }

    private static NotificationOutboxHandler CreateHandler(
        ApplicationDbContext context,
        INotificationRealtimeNotifier notifier) => new(
            context,
            notifier,
            [
                new ContractNotificationEventMapper(
                    new ContractNotificationContextReader(context))
            ]);

    private static Contract AddContract(ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد اختبار الإشعارات",
            "شروط عقد صالحة لاختبار إشعارات دورة حياة العقد.",
            UtcNow);
        context.Contracts.Add(contract);
        return contract;
    }

    private static OutboxMessage CreateMessage<T>(
        string eventType,
        int eventVersion,
        Guid aggregateId,
        T payload) => new(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            JsonSerializer.Serialize(payload),
            "Contract",
            aggregateId,
            Guid.NewGuid(),
            UtcNow,
            UtcNow);

    private static ApplicationDbContext CreateContext()
    {
        return new ApplicationDbContext(
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase($"contract-notifications-{Guid.NewGuid():N}")
                .Options);
    }

    private static void AssertMetadata(
        NotificationDto notification,
        Contract contract)
    {
        Assert.Equal(contract.Id.ToString(), notification.Data!["contractId"]);
        Assert.Equal(
            contract.ProposalId.ToString(),
            notification.Data["proposalId"]);
        Assert.Equal(
            contract.LegalCaseId.ToString(),
            notification.Data["legalCaseId"]);
    }

    private sealed class RecordingNotifier : INotificationRealtimeNotifier
    {
        public List<(Guid UserId, NotificationDto Notification)> Created { get; } = [];

        public Task NotificationCreatedAsync(
            Guid userId,
            NotificationDto notification,
            CancellationToken cancellationToken)
        {
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
