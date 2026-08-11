using System.Globalization;
using System.Text.Json;
using SmartCourt.Common.Enums;
using SmartCourt.Features.Admin.Verifications.Integration;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class VerificationNotificationEventMapper(
    IVerificationNotificationContextReader contextReader)
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        VerificationEventTypes.DocumentApproved,
        VerificationEventTypes.DocumentRejected,
        VerificationEventTypes.DocumentExpired,
        VerificationEventTypes.AccountApproved,
        VerificationEventTypes.AccountRejected,
        VerificationEventTypes.ReviewRequested
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        EnsureVersion(message, 1);

        return message.EventType switch
        {
            VerificationEventTypes.DocumentApproved
                or VerificationEventTypes.DocumentRejected
                or VerificationEventTypes.DocumentExpired
                => await MapDocumentAsync(message, cancellationToken),
            VerificationEventTypes.AccountApproved
                or VerificationEventTypes.AccountRejected
                => await MapAccountAsync(message, cancellationToken),
            VerificationEventTypes.ReviewRequested
                => await MapReviewRequestedAsync(message, cancellationToken),
            _ => throw Unsupported(message.EventType)
        };
    }

    private async Task<IReadOnlyCollection<NotificationDraft>> MapDocumentAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<VerificationDocumentEventPayload>(message);
        EnsureIdentifier(payload.DocumentId, "document");
        EnsureIdentifier(payload.UserId, "user");
        EnsureAggregate(message, payload.DocumentId);

        var context = await contextReader.GetDocumentAsync(
            payload.DocumentId,
            cancellationToken);
        if (context.DocumentId != payload.DocumentId
            || context.UserId != payload.UserId
            || !string.Equals(
                context.DocumentType.ToString(),
                payload.DocumentType,
                StringComparison.Ordinal)
            || !string.Equals(
                context.Status.ToString(),
                payload.Status,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Verification notification payload does not match its authoritative context.");
        }

        var contract = message.EventType switch
        {
            VerificationEventTypes.DocumentApproved =>
                new DocumentNotificationContract(
                    "verification.document-approved",
                    NotificationSeverity.Success,
                    "تم اعتماد مستند التحقق",
                    "تم اعتماد أحد مستندات التحقق الخاصة بك. يمكنك متابعة حالة التحقق من حسابك.",
                    VerificationDocumentStatus.Verified),
            VerificationEventTypes.DocumentRejected =>
                new DocumentNotificationContract(
                    "verification.document-rejected",
                    NotificationSeverity.Warning,
                    "تم رفض مستند التحقق",
                    "تم رفض أحد مستندات التحقق الخاصة بك. يرجى مراجعة التفاصيل واستبدال المستند عند الحاجة.",
                    VerificationDocumentStatus.Rejected),
            VerificationEventTypes.DocumentExpired =>
                new DocumentNotificationContract(
                    "verification.document-expired",
                    NotificationSeverity.Warning,
                    "انتهت صلاحية مستند التحقق",
                    "انتهت صلاحية أحد مستندات التحقق الخاصة بك. يرجى إعادة رفع مستند ساري المفعول.",
                    VerificationDocumentStatus.Expired),
            _ => throw Unsupported(message.EventType)
        };

        if (context.Status != contract.ExpectedStatus)
        {
            throw new InvalidOperationException(
                "Verification document notification event does not match its expected status.");
        }

        return
        [
            new NotificationDraft(
                context.UserId,
                contract.Type,
                contract.Severity,
                contract.Title,
                contract.Body,
                null,
                new Dictionary<string, string>
                {
                    ["documentId"] = context.DocumentId.ToString(),
                    ["documentType"] = context.DocumentType.ToString()
                })
        ];
    }

    private async Task<IReadOnlyCollection<NotificationDraft>> MapAccountAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<VerificationAccountEventPayload>(message);
        EnsureIdentifier(payload.UserId, "user");
        EnsureAggregate(message, payload.UserId);

        var context = await contextReader.GetAccountAsync(
            payload.UserId,
            cancellationToken);
        if (context.UserId != payload.UserId
            || !string.Equals(
                context.Status.ToString(),
                payload.Status,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Verification account notification payload does not match its authoritative context.");
        }

        var contract = message.EventType switch
        {
            VerificationEventTypes.AccountApproved =>
                new AccountNotificationContract(
                    "account.approved",
                    NotificationSeverity.Success,
                    "تم اعتماد حسابك",
                    "تم اعتماد حسابك وأصبح جاهزًا للاستخدام.",
                    UserStatus.Active),
            VerificationEventTypes.AccountRejected =>
                new AccountNotificationContract(
                    "account.rejected",
                    NotificationSeverity.Critical,
                    "تم رفض الحساب",
                    "تم رفض طلب اعتماد حسابك. يرجى مراجعة التفاصيل واتخاذ الإجراء المطلوب.",
                    UserStatus.Rejected),
            _ => throw Unsupported(message.EventType)
        };

        if (!string.Equals(
                payload.Status,
                contract.ExpectedStatus.ToString(),
                StringComparison.Ordinal)
            || context.Status != contract.ExpectedStatus)
        {
            throw new InvalidOperationException(
                "Verification account notification event does not match its expected status.");
        }

        return
        [
            new NotificationDraft(
                context.UserId,
                contract.Type,
                contract.Severity,
                contract.Title,
                contract.Body,
                null,
                new Dictionary<string, string>
                {
                    ["userId"] = context.UserId.ToString()
                })
        ];
    }

    private async Task<IReadOnlyCollection<NotificationDraft>> MapReviewRequestedAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<VerificationReviewRequestedEventPayload>(message);
        EnsureIdentifier(payload.UserId, "user");
        EnsureAggregate(message, payload.UserId);
        if (payload.DocumentCount <= 0)
        {
            throw new InvalidOperationException(
                "Verification review-requested notification document count is invalid.");
        }

        var context = await contextReader.GetReviewRequestedAsync(
            payload.UserId,
            cancellationToken);
        if (context.UserId != payload.UserId)
        {
            throw new InvalidOperationException(
                "Verification review-requested notification payload does not match its authoritative context.");
        }

        var data = new Dictionary<string, string>
        {
            ["userId"] = context.UserId.ToString(),
            ["documentCount"] = payload.DocumentCount.ToString(CultureInfo.InvariantCulture)
        };

        return context.AdministratorUserIds
            .Distinct()
            .Select(administratorUserId =>
            {
                EnsureIdentifier(administratorUserId, "administrator");
                return new NotificationDraft(
                    administratorUserId,
                    "verification.review-requested",
                    NotificationSeverity.Information,
                    "طلب مراجعة مستندات التحقق",
                    "تم رفع مستندات تحقق جديدة لأحد المستخدمين. يرجى مراجعتها واتخاذ الإجراء المناسب.",
                    null,
                    data);
            })
            .ToArray();
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
                "Verification notification payload is invalid.",
                exception);
        }
    }

    private static void EnsureVersion(
        OutboxMessage message,
        int expectedVersion)
    {
        if (message.EventVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Verification notification event version {message.EventVersion} is unsupported for '{message.EventType}'.");
        }
    }

    private static void EnsureAggregate(
        OutboxMessage message,
        Guid aggregateId)
    {
        if (aggregateId == Guid.Empty || aggregateId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Verification notification aggregate and payload identifiers do not match.");
        }
    }

    private static void EnsureIdentifier(Guid identifier, string name)
    {
        if (identifier == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Verification notification {name} identifier is invalid.");
        }
    }

    private static InvalidOperationException Unsupported(string eventType) =>
        new($"Verification notification event type '{eventType}' is unsupported.");

    private sealed record DocumentNotificationContract(
        string Type,
        NotificationSeverity Severity,
        string Title,
        string Body,
        VerificationDocumentStatus ExpectedStatus);

    private sealed record AccountNotificationContract(
        string Type,
        NotificationSeverity Severity,
        string Title,
        string Body,
        UserStatus ExpectedStatus);
}
