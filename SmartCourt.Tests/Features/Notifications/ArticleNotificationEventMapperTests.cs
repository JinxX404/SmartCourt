using System.Text.Json;
using SmartCourt.Features.Articles;
using SmartCourt.Features.Articles.Events;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Infrastructure.Persistence.Entities;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public class ArticleNotificationEventMapperTests
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    private class StubContextReader : IArticleNotificationContextReader
    {
        public List<Guid> AdminUserIds { get; set; } = new();

        public Task<IReadOnlyCollection<Guid>> GetAdminUserIdsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IReadOnlyCollection<Guid>>(AdminUserIds);
        }
    }

    private static OutboxMessage CreateMessage(string eventType, int eventVersion, string payload, Guid aggregateId)
    {
        return new OutboxMessage(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            payload,
            "LegalArticle",
            aggregateId,
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow);
    }

    [Fact]
    public void EventTypes_ContainsAllArticleNotificationEvents()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());

        Assert.Contains(ArticleEventTypes.ArticleCommentAdded, mapper.EventTypes);
        Assert.Contains(ArticleEventTypes.ArticleReported, mapper.EventTypes);
        Assert.Contains(ArticleEventTypes.ArticleDeletedByAdmin, mapper.EventTypes);
        Assert.Contains(ArticleEventTypes.ArticleLikeThresholdReached, mapper.EventTypes);
    }

    [Fact]
    public async Task MapAsync_WithUnsupportedVersion_ThrowsInvalidOperationException()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var message = CreateMessage(ArticleEventTypes.ArticleCommentAdded, 2, "{}", Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_WithUnsupportedEventType_ThrowsInvalidOperationException()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var message = CreateMessage("UnknownEvent", 1, "{}", Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_ArticleCommentAdded_ReturnsDraft_WhenCommenterIsNotAuthor()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var articleId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var commenterId = Guid.NewGuid();

        var payload = new ArticleCommentAddedV1(articleId, commentId, authorId, commenterId);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleCommentAdded, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Single(drafts);
        var draft = drafts.First();
        
        Assert.Equal(authorId, draft.RecipientUserId);
        Assert.Equal("article.comment-added", draft.Type);
        Assert.Equal(NotificationSeverity.Information, draft.Severity);
        Assert.Equal($"/articles/{articleId}", draft.ActionUrl);
        Assert.Equal(articleId.ToString(), draft.Data!["articleId"]);
        Assert.Equal(commentId.ToString(), draft.Data!["commentId"]);
        Assert.Equal("تعليق جديد على مقالك", draft.Title);
    }

    [Fact]
    public async Task MapAsync_ArticleCommentAdded_ReturnsEmpty_WhenCommenterIsAuthor()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var articleId = Guid.NewGuid();
        var commentId = Guid.NewGuid();
        var authorId = Guid.NewGuid();

        var payload = new ArticleCommentAddedV1(articleId, commentId, authorId, authorId);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleCommentAdded, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Empty(drafts);
    }

    [Fact]
    public async Task MapAsync_ArticleReported_ReturnsDraftsForAdmins()
    {
        var admin1 = Guid.NewGuid();
        var admin2 = Guid.NewGuid();
        var stubReader = new StubContextReader { AdminUserIds = new List<Guid> { admin1, admin2 } };
        var mapper = new ArticleNotificationEventMapper(stubReader);

        var articleId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var payload = new ArticleReportedV1(articleId, reportId);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleReported, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Contains(drafts, d => d.RecipientUserId == admin1);
        Assert.Contains(drafts, d => d.RecipientUserId == admin2);

        var sample = drafts.First();
        Assert.Equal("article.reported", sample.Type);
        Assert.Equal(NotificationSeverity.Warning, sample.Severity);
        Assert.Equal($"/articles/{articleId}", sample.ActionUrl);
        Assert.Equal(articleId.ToString(), sample.Data!["articleId"]);
        Assert.Equal(reportId.ToString(), sample.Data!["reportId"]);
        Assert.Equal("بلاغ جديد على مقال", sample.Title);
    }

    [Fact]
    public async Task MapAsync_ArticleReported_ReturnsEmpty_WhenNoAdmins()
    {
        var stubReader = new StubContextReader(); // Empty admins
        var mapper = new ArticleNotificationEventMapper(stubReader);

        var articleId = Guid.NewGuid();
        var reportId = Guid.NewGuid();

        var payload = new ArticleReportedV1(articleId, reportId);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleReported, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Empty(drafts);
    }

    [Fact]
    public async Task MapAsync_ArticleDeletedByAdmin_ReturnsDraftWithTitle()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var articleId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var title = "Test Article";

        var payload = new ArticleDeletedByAdminV1(articleId, authorId, title);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleDeletedByAdmin, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Single(drafts);
        var draft = drafts.First();
        
        Assert.Equal(authorId, draft.RecipientUserId);
        Assert.Equal("article.deleted-by-admin", draft.Type);
        Assert.Equal(NotificationSeverity.Warning, draft.Severity);
        Assert.Equal($"/articles/{articleId}", draft.ActionUrl);
        Assert.Equal(articleId.ToString(), draft.Data!["articleId"]);
        Assert.Equal("تم حذف مقالك من قبل الإدارة", draft.Title);
        Assert.Contains($"بعنوان \"{title}\"", draft.Body);
    }

    [Fact]
    public async Task MapAsync_ArticleLikeThresholdReached_ReturnsDraft()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var articleId = Guid.NewGuid();
        var authorId = Guid.NewGuid();
        var likesCount = 10;

        var payload = new ArticleLikeThresholdReachedV1(articleId, authorId, likesCount);
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleLikeThresholdReached, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            articleId);

        var drafts = await mapper.MapAsync(message, CancellationToken.None);

        Assert.Single(drafts);
        var draft = drafts.First();
        
        Assert.Equal(authorId, draft.RecipientUserId);
        Assert.Equal("article.like-threshold-reached", draft.Type);
        Assert.Equal(NotificationSeverity.Success, draft.Severity);
        Assert.Equal($"/articles/{articleId}", draft.ActionUrl);
        Assert.Equal(articleId.ToString(), draft.Data!["articleId"]);
        Assert.Equal($"وصل مقالك إلى {likesCount} إعجاب.", draft.Body);
    }

    [Fact]
    public async Task MapAsync_WithAggregateIdMismatch_ThrowsInvalidOperationException()
    {
        var mapper = new ArticleNotificationEventMapper(new StubContextReader());
        var articleId = Guid.NewGuid();
        var payload = new ArticleReportedV1(articleId, Guid.NewGuid());
        
        var message = CreateMessage(
            ArticleEventTypes.ArticleReported, 
            1, 
            JsonSerializer.Serialize(payload, SerializerOptions), 
            Guid.NewGuid());

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => mapper.MapAsync(message, CancellationToken.None));
    }
}
