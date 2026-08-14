namespace SmartCourt.Features.Articles.Events;

public static class ArticleEventTypes
{
    public const string ArticleCommentAdded = "ArticleCommentAdded";
    public const string ArticleReported = "ArticleReported";
    public const string ArticleDeletedByAdmin = "ArticleDeletedByAdmin";
    public const string ArticleLikeThresholdReached = "ArticleLikeThresholdReached";
}
