using System;

namespace SmartCourt.Common.Entities;

public class ArticleComment
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArticleId { get; set; }
    public LegalArticle Article { get; set; } = null!;
    
    public Guid UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsDeleted { get; set; }
}
