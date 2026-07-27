using SmartCourt.Infrastructure.Persistence.Enums;

namespace SmartCourt.Infrastructure.Idempotency;

public enum IdempotencyReservationState
{
    Acquired = 0,
    Replay = 1
}

public sealed record IdempotencyReservation(
    Guid RecordId,
    IdempotencyReservationState State,
    string RequestHash,
    IdempotencyStatus Status,
    int? ResponseStatusCode,
    string? ResponseBody,
    Guid? ResultReferenceId)
{
    public bool IsReplay =>
        State == IdempotencyReservationState.Replay;
}
