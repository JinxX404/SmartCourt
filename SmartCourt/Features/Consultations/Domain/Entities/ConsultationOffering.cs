using SmartCourt.Common.Enums;
using SmartCourt.Features.Consultations.Domain.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationOffering
{
    public Guid Id { get; set; }
    public Guid LawyerId { get; set; }
    public ConsultationMode Mode { get; set; }
    public Specialization Specialization { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EGP";
    public string? OfficeLocation { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ConsultationOfferingInclusion> Inclusions { get; set; } = [];
}
