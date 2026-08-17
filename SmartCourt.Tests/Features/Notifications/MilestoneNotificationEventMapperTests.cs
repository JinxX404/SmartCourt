using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Milestones.Events;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class MilestoneNotificationEventMapperTests
{
    private static readonly DateTime UtcNow = new(
        2026,
        8,
        9,
        12,
        0,
        0,
        DateTimeKind.Utc);

    public static TheoryData<string, bool, string, string, string, string>
        ParticipantCases => new()
        {
            {
                ContractPaymentEventTypes.MilestoneCreated,
                true,
                "milestone.created",
                "Information",
                "مرحلة تعاقدية جديدة",
                "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها."
            },
            {
                ContractPaymentEventTypes.MilestoneCreated,
                false,
                "milestone.created",
                "Information",
                "مرحلة تعاقدية جديدة",
                "أضاف الطرف الآخر مرحلة جديدة إلى العقد لتراجع شروطها."
            },
            {
                ContractPaymentEventTypes.MilestoneDraftUpdated,
                true,
                "milestone.draft-updated",
                "Warning",
                "تم تحديث المرحلة",
                "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك."
            },
            {
                ContractPaymentEventTypes.MilestoneDraftUpdated,
                false,
                "milestone.draft-updated",
                "Warning",
                "تم تحديث المرحلة",
                "تم تحديث شروط المرحلة، وتحتاج النسخة الحالية إلى مراجعتك وموافقتك."
            },
            {
                ContractPaymentEventTypes.MilestoneAcceptanceRecorded,
                true,
                "milestone.acceptance-recorded",
                "Information",
                "موافقة جديدة على المرحلة",
                "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك."
            },
            {
                ContractPaymentEventTypes.MilestoneAcceptanceRecorded,
                false,
                "milestone.acceptance-recorded",
                "Information",
                "موافقة جديدة على المرحلة",
                "وافق الطرف الآخر على شروط المرحلة الحالية، والمرحلة بانتظار موافقتك."
            }
        };

    [Theory]
    [MemberData(nameof(ParticipantCases))]
    public async Task HandleAsync_ParticipantEventNotifiesOnlyCounterparty(
        string eventType,
        bool clientActs,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var actorUserId = clientActs
            ? contract.ClientUserId
            : contract.LawyerUserId;
        var expectedRecipient = clientActs
            ? contract.LawyerUserId
            : contract.ClientUserId;
        var message = CreateMessage(
            eventType,
            milestone.Id,
            new MilestoneParticipantEventPayload(
                milestone.Id,
                actorUserId));

        await CreateHandler(context).HandleAsync(
            message,
            CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(expectedRecipient, saved.RecipientUserId);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal(expectedSeverity, saved.Severity.ToString());
        Assert.Equal(expectedTitle, saved.Title);
        Assert.Equal(expectedBody, saved.Body);
        Assert.Equal(
            $"/dashboard/contracts/{contract.Id}?milestoneId={milestone.Id}",
            saved.ActionUrl);
        AssertMetadata(saved.DataJson, contract, milestone);
    }

    [Fact]
    public async Task HandleAsync_ApprovedNotifiesBothParticipants()
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneApproved,
            milestone.Id,
            new ContractPaymentAggregateEventPayload(milestone.Id));

        await CreateHandler(context).HandleAsync(
            message,
            CancellationToken.None);

        var saved = await context.Notifications.ToListAsync();
        Assert.Equal(2, saved.Count);
        Assert.Contains(saved, item =>
            item.RecipientUserId == contract.ClientUserId);
        Assert.Contains(saved, item =>
            item.RecipientUserId == contract.LawyerUserId);
        Assert.All(saved, item =>
        {
            Assert.Equal("milestone.approved", item.Type);
            Assert.Equal("Success", item.Severity.ToString());
            Assert.Equal("تم اعتماد المرحلة", item.Title);
            Assert.Equal(
                "وافق طرفا العقد على شروط المرحلة وأصبحت جاهزة للانتقال إلى التمويل.",
                item.Body);
            Assert.Equal(
                $"/dashboard/contracts/{contract.Id}?milestoneId={milestone.Id}",
                item.ActionUrl);
        });
    }

    [Theory]
    [InlineData(
        ContractPaymentEventTypes.MilestoneReadyForFunding,
        true,
        "milestone.ready-for-funding",
        "Information",
        "المرحلة جاهزة للتمويل",
        "أصبحت المرحلة جاهزة للتمويل حتى يتمكن المحامي من بدء العمل.")]
    [InlineData(
        ContractPaymentEventTypes.MilestoneSubmitted,
        true,
        "milestone.submitted",
        "Information",
        "تم تسليم أعمال المرحلة",
        "سلّم المحامي أعمال المرحلة، ويمكنك الآن مراجعتها وقبولها أو طلب تعديلات.")]
    [InlineData(
        ContractPaymentEventTypes.MilestoneChangesRequested,
        false,
        "milestone.changes-requested",
        "Warning",
        "طُلبت تعديلات على المرحلة",
        "طلب العميل تعديلات على أعمال المرحلة، ويمكنك مراجعة الطلب وإعادة التسليم.")]
    [InlineData(
        ContractPaymentEventTypes.MilestoneAccepted,
        false,
        "milestone.accepted",
        "Success",
        "تم قبول أعمال المرحلة",
        "قبل العميل أعمال المرحلة، وبدأت مدة حجز المبلغ قبل إتاحته للصرف.")]
    public async Task HandleAsync_ExecutionEventUsesAuthoritativeRecipient(
        string eventType,
        bool recipientIsClient,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            eventType,
            milestone.Id,
            ExecutionPayload(eventType, milestone.Id));

        await CreateHandler(context).HandleAsync(
            message,
            CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        Assert.Equal(
            recipientIsClient
                ? contract.ClientUserId
                : contract.LawyerUserId,
            saved.RecipientUserId);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal(expectedSeverity, saved.Severity.ToString());
        Assert.Equal(expectedTitle, saved.Title);
        Assert.Equal(expectedBody, saved.Body);
        Assert.Equal(
            $"/dashboard/contracts/{contract.Id}?milestoneId={milestone.Id}",
            saved.ActionUrl);
    }

    [Fact]
    public async Task HandleAsync_AutoAcceptedUsesRoleSpecificCopy()
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneAutoAccepted,
            milestone.Id,
            new MilestoneAutoAcceptedEventPayload(
                milestone.Id,
                Guid.NewGuid(),
                1));

        await CreateHandler(context).HandleAsync(
            message,
            CancellationToken.None);

        var saved = await context.Notifications.ToListAsync();
        var client = Assert.Single(
            saved,
            item => item.RecipientUserId == contract.ClientUserId);
        Assert.Equal("Warning", client.Severity.ToString());
        Assert.Equal(
            "انتهت مدة المراجعة وقُبلت أعمال المرحلة تلقائيًا، وبدأت مدة الاعتراض.",
            client.Body);
        var lawyer = Assert.Single(
            saved,
            item => item.RecipientUserId == contract.LawyerUserId);
        Assert.Equal("Success", lawyer.Severity.ToString());
        Assert.Equal(
            "قُبلت أعمال المرحلة تلقائيًا بعد انتهاء مدة المراجعة، وبدأت مدة حجز المبلغ.",
            lawyer.Body);
        Assert.All(saved, item =>
        {
            Assert.Equal("milestone.auto-accepted", item.Type);
            Assert.Equal("تم قبول المرحلة تلقائيًا", item.Title);
        });
    }

    public static TheoryData<string, bool, bool, string, string, string>
        ChangeRequestCases => new()
        {
            {
                ContractPaymentEventTypes.MilestoneChangeRequestCreated,
                true,
                false,
                "milestone.change-request-created",
                "Information",
                "طلب تعديل جديد للمرحلة"
            },
            {
                ContractPaymentEventTypes.MilestoneChangeRequestCreated,
                false,
                false,
                "milestone.change-request-created",
                "Information",
                "طلب تعديل جديد للمرحلة"
            },
            {
                ContractPaymentEventTypes.MilestoneChangeRequestApproved,
                true,
                true,
                "milestone.change-request-approved",
                "Success",
                "تمت الموافقة على طلب التعديل"
            },
            {
                ContractPaymentEventTypes.MilestoneChangeRequestRejected,
                false,
                true,
                "milestone.change-request-rejected",
                "Warning",
                "تم رفض طلب تعديل المرحلة"
            },
            {
                ContractPaymentEventTypes.MilestoneChangeRequestCancelled,
                true,
                false,
                "milestone.change-request-cancelled",
                "Information",
                "تم إلغاء طلب تعديل المرحلة"
            },
            {
                ContractPaymentEventTypes.MilestoneChangeRequestCancelled,
                false,
                false,
                "milestone.change-request-cancelled",
                "Information",
                "تم إلغاء طلب تعديل المرحلة"
            }
        };

    [Theory]
    [MemberData(nameof(ChangeRequestCases))]
    public async Task HandleAsync_ChangeRequestUsesRequesterRelationship(
        string eventType,
        bool requesterIsClient,
        bool recipientIsRequester,
        string expectedType,
        string expectedSeverity,
        string expectedTitle)
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        var requester = requesterIsClient
            ? contract.ClientUserId
            : contract.LawyerUserId;
        var changeRequest = AddChangeRequest(context, milestone.Id, requester);
        SetChangeRequestState(changeRequest, eventType);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            eventType,
            changeRequest.Id,
            new MilestoneChangeRequestEventPayload(
                milestone.Id,
                changeRequest.Id,
                changeRequest.Status.ToString()),
            "MilestoneChangeRequest");

        await CreateHandler(context).HandleAsync(
            message,
            CancellationToken.None);

        var saved = await context.Notifications.SingleAsync();
        var counterparty = requesterIsClient
            ? contract.LawyerUserId
            : contract.ClientUserId;
        Assert.Equal(
            recipientIsRequester ? requester : counterparty,
            saved.RecipientUserId);
        Assert.Equal(expectedType, saved.Type);
        Assert.Equal(expectedSeverity, saved.Severity.ToString());
        Assert.Equal(expectedTitle, saved.Title);
        Assert.Equal(
            $"/dashboard/contracts/{contract.Id}?milestoneId={milestone.Id}",
            saved.ActionUrl);
        AssertMetadata(saved.DataJson, contract, milestone, changeRequest.Id);
    }

    [Fact]
    public async Task HandleAsync_NonParticipantActorFailsBeforePersistence()
    {
        await using var context = CreateContext();
        var (_, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneCreated,
            milestone.Id,
            new MilestoneParticipantEventPayload(
                milestone.Id,
                Guid.NewGuid()));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context).HandleAsync(
                message,
                CancellationToken.None));

        Assert.Contains("not a contract participant", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_AggregateMismatchFailsBeforePersistence()
    {
        await using var context = CreateContext();
        var (_, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneApproved,
            Guid.NewGuid(),
            new ContractPaymentAggregateEventPayload(milestone.Id));

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context).HandleAsync(
                message,
                CancellationToken.None));

        Assert.Contains("do not match", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_UnsupportedVersionFailsDeterministically()
    {
        await using var context = CreateContext();
        var (_, milestone) = AddMilestone(context);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneApproved,
            milestone.Id,
            new ContractPaymentAggregateEventPayload(milestone.Id),
            eventVersion: 99);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context).HandleAsync(
                message,
                CancellationToken.None));

        Assert.Contains("version 99 is unsupported", exception.Message);
        Assert.Empty(context.Notifications);
    }

    [Fact]
    public async Task HandleAsync_ChangeRequestStatusMismatchFails()
    {
        await using var context = CreateContext();
        var (contract, milestone) = AddMilestone(context);
        var changeRequest = AddChangeRequest(
            context,
            milestone.Id,
            contract.ClientUserId);
        await context.SaveChangesAsync();
        var message = CreateMessage(
            ContractPaymentEventTypes.MilestoneChangeRequestApproved,
            changeRequest.Id,
            new MilestoneChangeRequestEventPayload(
                milestone.Id,
                changeRequest.Id,
                "Pending"),
            "MilestoneChangeRequest");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateHandler(context).HandleAsync(
                message,
                CancellationToken.None));

        Assert.Contains("status does not match", exception.Message);
        Assert.Empty(context.Notifications);
    }

    private static NotificationOutboxHandler CreateHandler(
        ApplicationDbContext context) => new(
            context,
            new RecordingNotifier(),
            [
                new MilestoneNotificationEventMapper(
                    new MilestoneNotificationContextReader(context))
            ]);

    private static (Contract Contract, Milestone Milestone) AddMilestone(
        ApplicationDbContext context)
    {
        var contract = new Contract(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "عقد اختبار إشعارات المراحل",
            "شروط عقد صالحة لاختبار إشعارات المراحل.",
            UtcNow);
        var milestone = new Milestone(
            Guid.NewGuid(),
            contract.Id,
            "مرحلة الاختبار",
            "وصف المرحلة",
            1,
            1_000m,
            14,
            UtcNow.AddDays(14),
            UtcNow);
        context.Contracts.Add(contract);
        context.Milestones.Add(milestone);
        return (contract, milestone);
    }

    private static MilestoneChangeRequest AddChangeRequest(
        ApplicationDbContext context,
        Guid milestoneId,
        Guid requesterUserId)
    {
        var changeRequest = new MilestoneChangeRequest(
            Guid.NewGuid(),
            milestoneId,
            requesterUserId,
            "وصف معدل للمرحلة",
            null,
            null,
            "سبب آمن للاختبار",
            UtcNow);
        context.MilestoneChangeRequests.Add(changeRequest);
        return changeRequest;
    }

    private static void SetChangeRequestState(
        MilestoneChangeRequest changeRequest,
        string eventType)
    {
        changeRequest.Status = eventType switch
        {
            ContractPaymentEventTypes.MilestoneChangeRequestCreated =>
                ChangeRequestStatus.Pending,
            ContractPaymentEventTypes.MilestoneChangeRequestApproved =>
                ChangeRequestStatus.Approved,
            ContractPaymentEventTypes.MilestoneChangeRequestRejected =>
                ChangeRequestStatus.Rejected,
            ContractPaymentEventTypes.MilestoneChangeRequestCancelled =>
                ChangeRequestStatus.Cancelled,
            _ => throw new InvalidOperationException()
        };
    }

    private static object ExecutionPayload(string eventType, Guid milestoneId)
    {
        return eventType switch
        {
            ContractPaymentEventTypes.MilestoneSubmitted =>
                new MilestoneSubmissionEventPayload(
                    milestoneId,
                    Guid.NewGuid(),
                    1),
            ContractPaymentEventTypes.MilestoneAccepted =>
                new MilestoneAcceptanceEventPayload(
                    milestoneId,
                    Guid.NewGuid()),
            _ => new ContractPaymentAggregateEventPayload(milestoneId)
        };
    }

    private static OutboxMessage CreateMessage<T>(
        string eventType,
        Guid aggregateId,
        T payload,
        string aggregateType = "Milestone",
        int eventVersion = 1) => new(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            JsonSerializer.Serialize(payload),
            aggregateType,
            aggregateId,
            Guid.NewGuid(),
            UtcNow,
            UtcNow);

    private static ApplicationDbContext CreateContext() => new(
        new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(
                $"milestone-notifications-{Guid.NewGuid():N}")
            .Options);

    private static void AssertMetadata(
        string? dataJson,
        Contract contract,
        Milestone milestone,
        Guid? changeRequestId = null)
    {
        Assert.NotNull(dataJson);
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(
            dataJson,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(data);
        Assert.Equal(milestone.Id.ToString(), data["milestoneId"]);
        Assert.Equal(contract.Id.ToString(), data["contractId"]);
        Assert.Equal(contract.ProposalId.ToString(), data["proposalId"]);
        Assert.Equal(contract.LegalCaseId.ToString(), data["legalCaseId"]);
        if (changeRequestId.HasValue)
        {
            Assert.Equal(
                changeRequestId.Value.ToString(),
                data["changeRequestId"]);
        }
    }

    private sealed class RecordingNotifier : INotificationRealtimeNotifier
    {
        public Task NotificationCreatedAsync(
            Guid userId,
            NotificationDto notification,
            CancellationToken cancellationToken) => Task.CompletedTask;

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
