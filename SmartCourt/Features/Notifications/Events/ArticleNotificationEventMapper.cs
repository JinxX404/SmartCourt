using System.Text.Json;
using SmartCourt.Features.Articles;
using SmartCourt.Features.Articles.Events;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class ArticleNotificationEventMapper : INotificationEventMapper
{
    private readonly IArticleNotificationContextReader _contextReader;

    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public ArticleNotificationEventMapper(IArticleNotificationContextReader contextReader)
    {
        _contextReader = contextReader;
    }

    public IReadOnlyCollection<string> EventTypes =>
    [
        ArticleEventTypes.ArticleCommentAdded,
        ArticleEventTypes.ArticleReported,
        ArticleEventTypes.ArticleDeletedByAdmin,
        ArticleEventTypes.ArticleLikeThresholdReached
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        
        if (message.EventVersion != 1)
        {
            throw new InvalidOperationException(
                $"Article notification event version {message.EventVersion} is unsupported.");
        }

        return message.EventType switch
        {
            ArticleEventTypes.ArticleCommentAdded => MapCommentAdded(message),
            ArticleEventTypes.ArticleReported => await MapReportedAsync(message, cancellationToken),
            ArticleEventTypes.ArticleDeletedByAdmin => MapDeletedByAdmin(message),
            ArticleEventTypes.ArticleLikeThresholdReached => MapLikeThresholdReached(message),
            _ => throw new InvalidOperationException(
                $"Article notification event type '{message.EventType}' is unsupported.")
        };
    }

    private IReadOnlyCollection<NotificationDraft> MapCommentAdded(OutboxMessage message)
    {
        var payload = DeserializePayload<ArticleCommentAddedV1>(message);
        ValidateAggregateId(message.AggregateId, payload.ArticleId);

        if (payload.CommenterUserId == payload.AuthorUserId)
        {
            return Array.Empty<NotificationDraft>();
        }

        var draft = new NotificationDraft(
            RecipientUserId: payload.AuthorUserId,
            Type: "article.comment-added",
            Severity: NotificationSeverity.Information,
            Title: "تعليق جديد على مقالك",
            Body: "أضاف أحد المستخدمين تعليقًا على مقالك. يمكنك مراجعته والرد عليه.",
            ActionUrl: NotificationActionUrls.Article(payload.ArticleId),
            Data: new Dictionary<string, string>
            {
                ["articleId"] = payload.ArticleId.ToString(),
                ["commentId"] = payload.CommentId.ToString()
            });

        return [draft];
    }

    private async Task<IReadOnlyCollection<NotificationDraft>> MapReportedAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = DeserializePayload<ArticleReportedV1>(message);
        ValidateAggregateId(message.AggregateId, payload.ArticleId);

        var adminIds = await _contextReader.GetAdminUserIdsAsync(cancellationToken);
        if (adminIds.Count == 0)
        {
            return Array.Empty<NotificationDraft>();
        }

        var drafts = new List<NotificationDraft>(adminIds.Count);
        foreach (var adminId in adminIds)
        {
            drafts.Add(new NotificationDraft(
                RecipientUserId: adminId,
                Type: "article.reported",
                Severity: NotificationSeverity.Warning,
                Title: "بلاغ جديد على مقال",
                Body: "تم الإبلاغ عن مقال من قبل أحد المستخدمين. يرجى مراجعته واتخاذ الإجراء المناسب.",
                ActionUrl: NotificationActionUrls.ArticleReport(payload.ArticleId),
                Data: new Dictionary<string, string>
                {
                    ["articleId"] = payload.ArticleId.ToString(),
                    ["reportId"] = payload.ReportId.ToString()
                }));
        }

        return drafts;
    }

    private IReadOnlyCollection<NotificationDraft> MapDeletedByAdmin(OutboxMessage message)
    {
        var payload = DeserializePayload<ArticleDeletedByAdminV1>(message);
        ValidateAggregateId(message.AggregateId, payload.ArticleId);

        var titleContent = !string.IsNullOrWhiteSpace(payload.ArticleTitle)
            ? $" بعنوان \"{payload.ArticleTitle}\""
            : "";

        var draft = new NotificationDraft(
            RecipientUserId: payload.AuthorUserId,
            Type: "article.deleted-by-admin",
            Severity: NotificationSeverity.Warning,
            Title: "تم حذف مقالك من قبل الإدارة",
            Body: $"قامت إدارة المنصة بحذف أحد مقالاتك{titleContent}. يمكنك مراجعة التفاصيل من حسابك.",
            ActionUrl: NotificationActionUrls.ArticleListing,
            Data: new Dictionary<string, string>
            {
                ["articleId"] = payload.ArticleId.ToString()
            });

        return [draft];
    }

    private IReadOnlyCollection<NotificationDraft> MapLikeThresholdReached(OutboxMessage message)
    {
        var payload = DeserializePayload<ArticleLikeThresholdReachedV1>(message);
        ValidateAggregateId(message.AggregateId, payload.ArticleId);

        var draft = new NotificationDraft(
            RecipientUserId: payload.AuthorUserId,
            Type: "article.like-threshold-reached",
            Severity: NotificationSeverity.Success,
            Title: "تفاعل جديد مع مقالك",
            Body: $"وصل مقالك إلى {payload.LikesCount} إعجاب.",
            ActionUrl: NotificationActionUrls.Article(payload.ArticleId),
            Data: new Dictionary<string, string>
            {
                ["articleId"] = payload.ArticleId.ToString()
            });

        return [draft];
    }

    private static T DeserializePayload<T>(OutboxMessage message)
    {
        return JsonSerializer.Deserialize<T>(message.Payload, SerializerOptions)
            ?? throw new InvalidOperationException("Article notification payload is invalid.");
    }

    private static void ValidateAggregateId(Guid messageAggregateId, Guid payloadArticleId)
    {
        if (payloadArticleId == Guid.Empty || payloadArticleId != messageAggregateId)
        {
            throw new InvalidOperationException(
                "Article notification aggregate and payload identifiers do not match.");
        }
    }
}
