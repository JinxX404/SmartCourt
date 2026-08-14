using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.Articles.DTOs;

public class CreateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public IFormFile? FeaturedImage { get; set; }
    
    public Guid CategoryId { get; set; }
    
    public bool IsDraft { get; set; }
}
