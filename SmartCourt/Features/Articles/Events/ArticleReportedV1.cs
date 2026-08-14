namespace SmartCourt.Features.Articles.Events;

public sealed record ArticleReportedV1(
    Guid ArticleId,
    Guid ReportId);
