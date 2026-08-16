using SmartCourt.Common.Enums;
using SmartCourt.Features.Consultations.Domain.Enums;

namespace SmartCourt.Features.Consultations.Domain.Entities;

public sealed class ConsultationBooking
{
    public Guid Id { get; set; }
    public Guid OfferingId { get; set; }
    public Guid SlotId { get; set; }
    public Guid LawyerId { get; set; }
    public Guid ClientId { get; set; }
    public ConsultationMode Mode { get; set; }
    public Specialization Specialization { get; set; }
    public string OfferingTitle { get; set; } = string.Empty;
    public string OfferingDescription { get; set; } = string.Empty;
    public string InclusionsJson { get; set; } = "[]";
    public int DurationMinutes { get; set; }
    public decimal GrossAmount { get; set; }
    public decimal PlatformFeeAmount { get; set; }
    public decimal LawyerNetAmount { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Subject { get; set; } = string.Empty;
    public string MatterSummary { get; set; } = string.Empty;
    public string? OfficeLocation { get; set; }
    public string? MeetingUrl { get; set; }
    public DateTimeOffset StartAtUtc { get; set; }
    public DateTimeOffset EndAtUtc { get; set; }
    public ConsultationBookingStatus Status { get; set; }
    public DateTimeOffset PaymentExpiresAtUtc { get; set; }
    public DateTimeOffset? PerformedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public string? CancellationReason { get; set; }
    public string? DisputeReason { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
