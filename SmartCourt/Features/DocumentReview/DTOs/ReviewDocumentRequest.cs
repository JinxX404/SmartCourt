using Microsoft.AspNetCore.Http;

namespace SmartCourt.Features.DocumentReview.DTOs;

public class ReviewDocumentRequest
{
    public IFormFile File { get; set; } = null!;
    public string Query { get; set; } = string.Empty;
}
