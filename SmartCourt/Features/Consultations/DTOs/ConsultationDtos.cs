using SmartCourt.Common.Enums;
using SmartCourt.Features.Consultations.Domain.Enums;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Consultations.DTOs;

public sealed record ConsultationSettingsDto(
    Guid LawyerId,
    bool IsEnabled,
    int MinimumBookingNoticeHours,
    int MaximumAdvanceBookingDays,
    int BufferMinutes,
    string TimeZoneId);

public sealed record ConsultationOfferingDto(
    Guid Id,
    Guid LawyerId,
    ConsultationMode Mode,
    Specialization Specialization,
    string Title,
    string Description,
    int DurationMinutes,
    decimal Price,
    string Currency,
    string? OfficeLocation,
    bool IsActive,
    IReadOnlyList<string> Inclusions,
    DateTime? NextAvailableAtUtc);

public sealed record ConsultationLawyerDto(
    Guid LawyerId,
    string Name,
    string? ProfilePictureUrl,
    string? Governorate,
    string? City,
    decimal AverageRating,
    bool IsAcceptingConsultations,
    bool IsBookable,
    string? UnavailableReason,
    decimal StartingPrice,
    string Currency,
    DateTime? NextAvailableAtUtc,
    IReadOnlyList<ConsultationOfferingDto> Offerings);

public sealed record ConsultationPageDto<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalRecords,
    int TotalPages);

public sealed record ConsultationSlotDto(
    Guid Id,
    Guid OfferingId,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    ConsultationSlotStatus Status,
    DateTime? ReservedUntilUtc);

public sealed record ConsultationBookingDto(
    Guid Id,
    Guid OfferingId,
    Guid SlotId,
    Guid LawyerId,
    string LawyerName,
    Guid ClientId,
    string ClientName,
    string? ClientPhoneNumber,
    ConsultationMode Mode,
    Specialization Specialization,
    string OfferingTitle,
    IReadOnlyList<string> Inclusions,
    int DurationMinutes,
    decimal GrossAmount,
    decimal PlatformFeeAmount,
    decimal LawyerNetAmount,
    string Currency,
    string Subject,
    string MatterSummary,
    DateTime StartAtUtc,
    DateTime EndAtUtc,
    ConsultationBookingStatus Status,
    DateTime PaymentExpiresAtUtc,
    string? OfficeLocation,
    string? MeetingUrl,
    DateTime? PerformedAtUtc,
    DateTime? CompletedAtUtc,
    string? CancellationReason,
    string? DisputeReason,
    ConsultationPaymentDto? Payment,
    IReadOnlyList<string> PermittedActions);

public sealed record ConsultationPaymentDto(
    Guid TransactionId,
    Guid BookingId,
    PaymentOperationType OperationType,
    PaymentTransactionStatus Status,
    decimal Amount,
    string Currency,
    string? ClientActionType,
    string? ClientSecret,
    string? RedirectUrl,
    string? FailureReason,
    DateTime CreatedAt);

public sealed record ConsultationEscrowDto(
    Guid BookingId,
    decimal GrossAmount,
    decimal PlatformFeeAmount,
    decimal NetAmount,
    string Currency,
    EscrowHoldStatus Status,
    DateTime FundedAtUtc,
    DateTime? HoldStartsAtUtc,
    DateTime? HoldExpiresAtUtc,
    DateTime? SettledAtUtc);
