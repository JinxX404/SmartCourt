namespace SmartCourt.Features.Articles.DTOs;

public class CreateCategoryRequest
{
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
}
