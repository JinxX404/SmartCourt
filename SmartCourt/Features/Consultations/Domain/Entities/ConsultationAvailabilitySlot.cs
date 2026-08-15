using SmartCourt.Features.Consultations.Domain.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationAvailabilitySlot
{
    public Guid Id { get; set; }
    public Guid LawyerId { get; set; }
    public Guid OfferingId { get; set; }
    public ConsultationOffering Offering { get; set; } = null!;
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public ConsultationSlotStatus Status { get; set; }
    public DateTimeOffset? ReservedUntilUtc { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
