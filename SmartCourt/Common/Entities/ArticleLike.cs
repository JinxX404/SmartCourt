using System;

namespace SmartCourt.Common.Entities;

public class ArticleLike
{
    public Guid ArticleId { get; set; }
    public LegalArticle Article { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
