using SmartCourt.Common.Enums;
using SmartCourt.Features.Consultations.Domain.Enums;

namespace SmartCourt.Features.Consultations.DTOs;

public sealed record UpdateConsultationSettingsRequest(
    bool IsEnabled,
    int MinimumBookingNoticeHours = 2,
    int MaximumAdvanceBookingDays = 60,
    int BufferMinutes = 15,
    string TimeZoneId = "Africa/Cairo");

public sealed record CreateConsultationOfferingRequest(
    ConsultationMode Mode,
    Specialization Specialization,
    string Title,
    string Description,
    int DurationMinutes,
    decimal Price,
    string? OfficeLocation,
    IReadOnlyList<string> Inclusions,
    bool IsActive = false);

public sealed record UpdateConsultationOfferingRequest(
    ConsultationMode Mode,
    Specialization Specialization,
    string Title,
    string Description,
    int DurationMinutes,
    decimal Price,
    string? OfficeLocation,
    IReadOnlyList<string> Inclusions);

public sealed record SetConsultationOfferingStatusRequest(bool IsActive);

public sealed record CreateConsultationSlotItem(DateTimeOffset StartAtUtc);

public sealed record CreateConsultationSlotsRequest(
    IReadOnlyList<CreateConsultationSlotItem> Slots);

public sealed record CreateConsultationBookingRequest(
    Guid OfferingId,
    Guid SlotId,
    string Subject,
    string MatterSummary);

public sealed record CreateConsultationPaymentSessionRequest(
    string ConfirmationTokenReference);

public sealed record CancelConsultationBookingRequest(string Reason);

public sealed record MarkConsultationPerformedRequest(string? MeetingUrl = null);

public sealed record SetConsultationDeliveryDetailsRequest(string MeetingUrl);

public sealed record OpenConsultationDisputeRequest(string Reason);

public sealed record SettleConsultationDisputeRequest(
    decimal ClientRefundAmount,
    string Reason);

public sealed class ConsultationLawyerFilter
{
    public ConsultationMode[]? Modes { get; init; }
    public Specialization[]? Specializations { get; init; }
    public decimal? MinimumPrice { get; init; }
    public decimal? MaximumPrice { get; init; }
    public DateTimeOffset? AvailableFromUtc { get; init; }
    public DateTimeOffset? AvailableToUtc { get; init; }
    public string? Search { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 5;
}

public sealed class ConsultationBookingFilter
{
    public ConsultationBookingStatus[]? Statuses { get; init; }
    public DateTimeOffset? FromUtc { get; init; }
    public DateTimeOffset? ToUtc { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 5;
}
