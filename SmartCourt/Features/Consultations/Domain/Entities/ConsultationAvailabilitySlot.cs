using SmartCourt.Features.Consultations.Domain.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationAvailabilitySlot
{
    public Guid Id { get; set; }
    public Guid LawyerId { get; set; }
    public Guid OfferingId { get; set; }
    public ConsultationOffering Offering { get; set; } = null!;
    public DateTime StartAtUtc { get; set; }
    public DateTime EndAtUtc { get; set; }
    public ConsultationSlotStatus Status { get; set; }
    public DateTime? ReservedUntilUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
