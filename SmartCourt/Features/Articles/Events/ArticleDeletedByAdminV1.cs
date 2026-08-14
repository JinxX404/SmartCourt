namespace SmartCourt.Features.Articles.Events;

public sealed record ArticleDeletedByAdminV1(
    Guid ArticleId,
    Guid AuthorUserId,
    string ArticleTitle);
