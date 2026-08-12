using SmartCourt.Common.Enums;

namespace SmartCourt.Common.Entities;

public class LegalArticle : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public string? FeaturedImageUrl { get; set; }
    public int ViewCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool IsDeleted { get; set; }
    public bool IsDeletedByAdmin { get; set; }
    public ArticleStatus Status { get; set; } = ArticleStatus.Draft;

    public Guid CategoryId { get; set; }
    public LegalArticleCategory Category { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public ApplicationUser Author { get; set; } = null!;
}
