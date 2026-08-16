namespace SmartCourt.Features.Articles.Events;

public sealed record ArticleLikeThresholdReachedV1(
    Guid ArticleId,
    Guid AuthorUserId,
    int LikesCount);
