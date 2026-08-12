using System;

namespace SmartCourt.Common.Entities;

public class ArticleView
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArticleId { get; set; }
    public LegalArticle Article { get; set; } = null!;
    
    public Guid? UserId { get; set; }
    public ApplicationUser? User { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
