using System.Text.Json;
using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Admin.Verifications.Integration;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class VerificationNotificationEventMapperTests
{
    private static readonly Guid DocumentId = Guid.NewGuid();
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly Guid FirstAdministratorId = Guid.NewGuid();
    private static readonly Guid SecondAdministratorId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(
        2026,
        8,
        11,
        12,
        0,
        0,
        DateTimeKind.Utc);

    public static TheoryData<
        string,
        VerificationDocumentStatus,
        string,
        string,
        string,
        string> DocumentCases => new()
        {
            {
                VerificationEventTypes.DocumentApproved,
                VerificationDocumentStatus.Verified,
                "verification.document-approved",
                "Success",
                "تم اعتماد مستند التحقق",
                "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك."
            },
            {
                VerificationEventTypes.DocumentRejected,
                VerificationDocumentStatus.Rejected,
                "verification.document-rejected",
                "Warning",
                "تم رفض مستند التحقق",
                "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة."
            },
            {
                VerificationEventTypes.DocumentExpired,
                VerificationDocumentStatus.Expired,
                "verification.document-expired",
                "Warning",
                "انتهت صلاحية مستند التحقق",
                "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول."
            }
        };

    public static TheoryData<
        string,
        UserStatus,
        string,
        string,
        string,
        string> AccountCases => new()
        {
            {
                VerificationEventTypes.AccountApproved,
                UserStatus.Active,
                "account.approved",
                "Success",
                "تم اعتماد حسابك",
                "تم اعتماد حسابك وأصبح جاهزًا للاستخدام."
            },
            {
                VerificationEventTypes.AccountRejected,
                UserStatus.Rejected,
                "account.rejected",
                "Critical",
                "تم رفض الحساب",
                "تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب."
            }
        };

    [Theory]
    [MemberData(nameof(DocumentCases))]
    public async Task MapAsync_DocumentOutcomeUsesExactArabicAndSafeData(
        string eventType,
        VerificationDocumentStatus status,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        var mapper = CreateMapper(new VerificationDocumentNotificationContext(
            DocumentId,
            UserId,
            VerificationDocumentType.NationalIdFront,
            status));

        var draft = Assert.Single(await mapper.MapAsync(
            CreateDocumentMessage(
                eventType,
                new VerificationDocumentEventPayload(
                    DocumentId,
                    UserId,
                    VerificationDocumentType.NationalIdFront.ToString(),
                    status.ToString())),
            CancellationToken.None));

        Assert.Equal(UserId, draft.RecipientUserId);
        Assert.Equal(expectedType, draft.Type);
        Assert.Equal(expectedSeverity, draft.Severity.ToString());
        Assert.Equal(expectedTitle, draft.Title);
        Assert.Equal(expectedBody, draft.Body);
        Assert.Null(draft.ActionUrl);
        Assert.Equal(DocumentId.ToString(), draft.Data!["documentId"]);
        Assert.Equal(
            VerificationDocumentType.NationalIdFront.ToString(),
            draft.Data["documentType"]);
        Assert.Equal(2, draft.Data.Count);
        Assert.DoesNotContain(
            draft.Data.Keys,
            key => key.Contains("path", StringComparison.OrdinalIgnoreCase)
                || key.Contains("reason", StringComparison.OrdinalIgnoreCase)
                || key.Contains("content", StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [MemberData(nameof(AccountCases))]
    public async Task MapAsync_AccountOutcomeUsesExactArabicAndOnlyAccountId(
        string eventType,
        UserStatus status,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        var mapper = CreateMapper(account: new VerificationAccountNotificationContext(
            UserId,
            status));

        var draft = Assert.Single(await mapper.MapAsync(
            CreateAccountMessage(
                eventType,
                new VerificationAccountEventPayload(
                    UserId,
                    status.ToString())),
            CancellationToken.None));

        Assert.Equal(UserId, draft.RecipientUserId);
        Assert.Equal(expectedType, draft.Type);
        Assert.Equal(expectedSeverity, draft.Severity.ToString());
        Assert.Equal(expectedTitle, draft.Title);
        Assert.Equal(expectedBody, draft.Body);
        Assert.Null(draft.ActionUrl);
        Assert.Equal(UserId.ToString(), draft.Data!["userId"]);
        Assert.Single(draft.Data);
    }

    [Fact]
    public async Task MapAsync_ReviewRequestedCreatesOneSafeDraftPerAdminOnly()
    {
        var mapper = CreateMapper(review: new VerificationReviewRequestedNotificationContext(
            UserId,
            [FirstAdministratorId, SecondAdministratorId, FirstAdministratorId]));

        var drafts = await mapper.MapAsync(
            CreateReviewMessage(
                new VerificationReviewRequestedEventPayload(UserId, 2)),
            CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Equal(
            [FirstAdministratorId, SecondAdministratorId],
            drafts.Select(draft => draft.RecipientUserId));
        Assert.All(drafts, draft =>
        {
            Assert.Equal("verification.review-requested", draft.Type);
            Assert.Equal("Information", draft.Severity.ToString());
            Assert.Equal("طلب مراجعة مستندات التحقق", draft.Title);
            Assert.Equal(
                "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                draft.Body);
            Assert.Null(draft.ActionUrl);
            Assert.Equal(UserId.ToString(), draft.Data!["userId"]);
            Assert.Equal("2", draft.Data["documentCount"]);
            Assert.Equal(2, draft.Data.Count);
            Assert.DoesNotContain(
                draft.Data.Keys,
                key => key.Contains("path", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("content", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("email", StringComparison.OrdinalIgnoreCase)
                    || key.Contains("phone", StringComparison.OrdinalIgnoreCase));
        });
    }

    [Fact]
    public void EventTypes_AdvertisesEachVerificationEventExactlyOnce()
    {
        var mapper = CreateMapper();

        Assert.Equal(6, mapper.EventTypes.Count);
        Assert.Equal(6, mapper.EventTypes.Distinct().Count());
        Assert.Contains(VerificationEventTypes.DocumentApproved, mapper.EventTypes);
        Assert.Contains(VerificationEventTypes.DocumentRejected, mapper.EventTypes);
        Assert.Contains(VerificationEventTypes.DocumentExpired, mapper.EventTypes);
        Assert.Contains(VerificationEventTypes.AccountApproved, mapper.EventTypes);
        Assert.Contains(VerificationEventTypes.AccountRejected, mapper.EventTypes);
        Assert.Contains(VerificationEventTypes.ReviewRequested, mapper.EventTypes);
    }

    [Fact]
    public async Task MapAsync_RejectsUnsupportedVersionAggregateAndAuthoritativeMismatch()
    {
        var mapper = CreateMapper();
        var payload = new VerificationDocumentEventPayload(
            DocumentId,
            UserId,
            VerificationDocumentType.NationalIdFront.ToString(),
            VerificationDocumentStatus.Verified.ToString());

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateDocumentMessage(
                    VerificationEventTypes.DocumentApproved,
                    payload,
                    eventVersion: 2),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateDocumentMessage(
                    VerificationEventTypes.DocumentApproved,
                    payload,
                    aggregateId: Guid.NewGuid()),
                CancellationToken.None));

        var mismatchedMapper = CreateMapper(new VerificationDocumentNotificationContext(
            DocumentId,
            Guid.NewGuid(),
            VerificationDocumentType.NationalIdFront,
            VerificationDocumentStatus.Verified));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mismatchedMapper.MapAsync(
                CreateDocumentMessage(
                    VerificationEventTypes.DocumentApproved,
                    payload),
                CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_RejectsWrongOutcomeStatusAndEmptyRecipient()
    {
        var mapper = CreateMapper(new VerificationDocumentNotificationContext(
            DocumentId,
            UserId,
            VerificationDocumentType.NationalIdFront,
            VerificationDocumentStatus.Rejected));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateDocumentMessage(
                    VerificationEventTypes.DocumentApproved,
                    new VerificationDocumentEventPayload(
                        DocumentId,
                        UserId,
                        VerificationDocumentType.NationalIdFront.ToString(),
                        VerificationDocumentStatus.Verified.ToString())),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateDocumentMessage(
                    VerificationEventTypes.DocumentRejected,
                    new VerificationDocumentEventPayload(
                        DocumentId,
                        Guid.Empty,
                        VerificationDocumentType.NationalIdFront.ToString(),
                        VerificationDocumentStatus.Rejected.ToString())),
                CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_RejectsInvalidReviewRequestPayload()
    {
        var mapper = CreateMapper(review: new VerificationReviewRequestedNotificationContext(
            UserId,
            [FirstAdministratorId]));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateReviewMessage(
                    new VerificationReviewRequestedEventPayload(UserId, 0)),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                CreateReviewMessage(
                    new VerificationReviewRequestedEventPayload(
                        Guid.NewGuid(),
                        1)),
                CancellationToken.None));
    }

    private static VerificationNotificationEventMapper CreateMapper(
        VerificationDocumentNotificationContext? document = null,
        VerificationAccountNotificationContext? account = null,
        VerificationReviewRequestedNotificationContext? review = null) =>
        new(new StubContextReader(
            document ?? new VerificationDocumentNotificationContext(
                DocumentId,
                UserId,
                VerificationDocumentType.NationalIdFront,
                VerificationDocumentStatus.Verified),
            account ?? new VerificationAccountNotificationContext(
                UserId,
                UserStatus.Active),
            review ?? new VerificationReviewRequestedNotificationContext(
                UserId,
                [FirstAdministratorId])));

    private static OutboxMessage CreateDocumentMessage(
        string eventType,
        VerificationDocumentEventPayload payload,
        int eventVersion = 1,
        Guid? aggregateId = null) => new(
        Guid.NewGuid(),
        eventType,
        eventVersion,
        JsonSerializer.Serialize(payload),
        nameof(UserVerificationDocument),
        aggregateId ?? DocumentId,
        Guid.NewGuid(),
        UtcNow,
        UtcNow);

    private static OutboxMessage CreateAccountMessage(
        string eventType,
        VerificationAccountEventPayload payload) => new(
        Guid.NewGuid(),
        eventType,
        1,
        JsonSerializer.Serialize(payload),
        nameof(ApplicationUser),
        UserId,
        Guid.NewGuid(),
        UtcNow,
        UtcNow);

    private static OutboxMessage CreateReviewMessage(
        VerificationReviewRequestedEventPayload payload,
        Guid? aggregateId = null) => new(
        Guid.NewGuid(),
        VerificationEventTypes.ReviewRequested,
        1,
        JsonSerializer.Serialize(payload),
        nameof(ApplicationUser),
        aggregateId ?? UserId,
        Guid.NewGuid(),
        UtcNow,
        UtcNow);

    private sealed class StubContextReader(
        VerificationDocumentNotificationContext document,
        VerificationAccountNotificationContext account,
        VerificationReviewRequestedNotificationContext review)
        : IVerificationNotificationContextReader
    {
        public Task<VerificationDocumentNotificationContext> GetDocumentAsync(
            Guid documentId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(DocumentId, documentId);
            return Task.FromResult(document);
        }

        public Task<VerificationAccountNotificationContext> GetAccountAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(UserId, userId);
            return Task.FromResult(account);
        }

        public Task<VerificationReviewRequestedNotificationContext> GetReviewRequestedAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(UserId, userId);
            return Task.FromResult(review);
        }
    }
}
