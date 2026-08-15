namespace SmartCourt.Features.Consultations.Domain.Enums;

public enum ConsultationBookingStatus : byte
{
    AwaitingPayment = 0,
    Confirmed = 1,
    AwaitingClientConfirmation = 2,
    Completed = 3,
    Cancelled = 4,
    Expired = 5,
    Disputed = 6,
    Refunded = 7
}
