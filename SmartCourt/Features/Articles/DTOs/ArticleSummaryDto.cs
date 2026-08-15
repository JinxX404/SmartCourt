using SmartCourt.Common.Enums;

namespace SmartCourt.Features.Articles.DTOs;

public class ArticleSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? FeaturedImageUrl { get; set; }
    public int ViewCount { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public ArticleStatus Status { get; set; }
    
    public Guid CategoryId { get; set; }
    public string CategoryNameAr { get; set; } = string.Empty;
    
    public Guid AuthorId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    
    public DateTimeOffset CreatedAt { get; set; }
}
