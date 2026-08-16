namespace SmartCourt.Features.Consultations.Domain.Enums;

public enum ConsultationSlotStatus : byte
{
    Available = 0,
    Reserved = 1,
    Booked = 2,
    Blocked = 3,
    Cancelled = 4
}
