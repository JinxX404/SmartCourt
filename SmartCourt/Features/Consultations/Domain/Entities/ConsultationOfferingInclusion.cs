namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationOfferingInclusion
{
    public Guid Id { get; set; }
    public Guid OfferingId { get; set; }
    public ConsultationOffering Offering { get; set; } = null!;
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
