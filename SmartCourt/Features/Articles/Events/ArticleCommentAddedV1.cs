namespace SmartCourt.Features.Articles.Events;

public sealed record ArticleCommentAddedV1(
    Guid ArticleId,
    Guid CommentId,
    Guid AuthorUserId,
    Guid CommenterUserId);
