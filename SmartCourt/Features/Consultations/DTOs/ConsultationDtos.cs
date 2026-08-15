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
    DateTimeOffset? NextAvailableAtUtc);

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
    DateTimeOffset? NextAvailableAtUtc,
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
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    ConsultationSlotStatus Status,
    DateTimeOffset? ReservedUntilUtc);

public sealed record ConsultationBookingDto(
    Guid Id,
    Guid OfferingId,
    Guid SlotId,
    Guid LawyerId,
    string LawyerName,
    Guid ClientId,
    string ClientName,
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
    DateTimeOffset StartAtUtc,
    DateTimeOffset EndAtUtc,
    ConsultationBookingStatus Status,
    DateTimeOffset PaymentExpiresAtUtc,
    string? OfficeLocation,
    string? MeetingUrl,
    DateTimeOffset? PerformedAtUtc,
    DateTimeOffset? CompletedAtUtc,
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
    DateTimeOffset CreatedAt);

public sealed record ConsultationEscrowDto(
    Guid BookingId,
    decimal GrossAmount,
    decimal PlatformFeeAmount,
    decimal NetAmount,
    string Currency,
    EscrowHoldStatus Status,
    DateTimeOffset FundedAtUtc,
    DateTimeOffset? HoldStartsAtUtc,
    DateTimeOffset? HoldExpiresAtUtc,
    DateTimeOffset? SettledAtUtc);
