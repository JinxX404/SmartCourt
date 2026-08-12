namespace SmartCourt.Features.Articles.DTOs;

public class CategoryDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string? Description { get; set; }
}
