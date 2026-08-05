namespace SmartCourt.Features.CaseReview.DTOs;

public class ReviewPointDto
{
    public Guid Id { get; set; }
    public string Description { get; set; } = null!;
    public string Type { get; set; } = null!;
}
