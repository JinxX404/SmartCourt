using System.Text.Json.Serialization;

namespace SmartCourt.Features.Articles.DTOs;

public class UpdateArticleRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public string? FeaturedImageUrl { get; set; }
    
    public Guid CategoryId { get; set; }
    
    [JsonPropertyName("isDraft")]
    public bool IsDraft { get; set; }
}
