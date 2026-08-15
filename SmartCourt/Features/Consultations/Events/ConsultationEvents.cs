using SmartCourt.Features.Consultations.Domain.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Consultations.Events;

public static class ConsultationEventTypes
{
    public const string BookingCreated = "consultation.booking.created.v1";
    public const string PaymentFunded = "consultation.payment.funded.v1";
    public const string BookingCancelled = "consultation.booking.cancelled.v1";
    public const string BookingExpired = "consultation.booking.expired.v1";
    public const string ConsultationPerformed = "consultation.performed.v1";
    public const string ConsultationCompleted = "consultation.completed.v1";
    public const string ConsultationDisputed = "consultation.disputed.v1";
    public const string DisputeSettled = "consultation.dispute.settled.v1";
    public const string PaymentReleased = "consultation.payment.released.v1";
}

public sealed record ConsultationEventPayload(
    Guid BookingId,
    Guid ClientUserId,
    Guid LawyerUserId,
    Guid? ActorUserId,
    string OfferingTitle,
    DateTimeOffset StartAtUtc);

internal static class ConsultationOutbox
{
    internal static Task EnqueueAsync(
        IOutboxWriter writer,
        string eventType,
        ConsultationBooking booking,
        Guid? actorUserId,
        CancellationToken cancellationToken) =>
        writer.EnqueueAsync(new OutboxEvent(
            eventType,
            1,
            new ConsultationEventPayload(
                booking.Id, booking.ClientId, booking.LawyerId,
                actorUserId, booking.OfferingTitle, booking.StartAtUtc),
            "ConsultationBooking",
            booking.Id,
            Guid.NewGuid()), cancellationToken);
}
