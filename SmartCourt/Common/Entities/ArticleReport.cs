using System;

namespace SmartCourt.Common.Entities;

public class ArticleReport
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArticleId { get; set; }
    public LegalArticle Article { get; set; } = null!;
    
    public Guid ReporterId { get; set; }
    public ApplicationUser Reporter { get; set; } = null!;

    public string Reason { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public bool IsResolved { get; set; }
}
