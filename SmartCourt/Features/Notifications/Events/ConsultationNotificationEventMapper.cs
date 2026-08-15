using System.Text.Json;
using SmartCourt.Features.Consultations.Events;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class ConsultationNotificationEventMapper : INotificationEventMapper
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ConsultationEventTypes.BookingCreated,
        ConsultationEventTypes.PaymentFunded,
        ConsultationEventTypes.BookingCancelled,
        ConsultationEventTypes.BookingExpired,
        ConsultationEventTypes.ConsultationPerformed,
        ConsultationEventTypes.ConsultationCompleted,
        ConsultationEventTypes.ConsultationDisputed,
        ConsultationEventTypes.DisputeSettled,
        ConsultationEventTypes.PaymentReleased
    ];

    public Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (message.EventVersion != 1)
            throw new InvalidOperationException("Unsupported consultation notification event version.");
        var payload = JsonSerializer.Deserialize<ConsultationEventPayload>(message.Payload, JsonOptions)
            ?? throw new InvalidOperationException("Consultation notification payload is invalid.");
        if (payload.BookingId != message.AggregateId)
            throw new InvalidOperationException("Consultation notification aggregate does not match its payload.");

        var data = new Dictionary<string, string>
        {
            ["bookingId"] = payload.BookingId.ToString(),
            ["startAtUtc"] = payload.StartAtUtc.ToString("O")
        };
        var url = $"/consultations/bookings/{payload.BookingId}";
        var drafts = message.EventType switch
        {
            ConsultationEventTypes.BookingCreated => One(payload.LawyerUserId,
                "consultation.booking.created", NotificationSeverity.Information,
                "New consultation booking", "A client reserved a consultation and is completing payment.", url, data),
            ConsultationEventTypes.PaymentFunded => Two(payload.ClientUserId, payload.LawyerUserId,
                "consultation.payment.funded", NotificationSeverity.Success,
                "Consultation confirmed", "The consultation payment was completed and the appointment is confirmed.", url, data),
            ConsultationEventTypes.BookingCancelled => One(OtherParticipant(payload),
                "consultation.booking.cancelled", NotificationSeverity.Warning,
                "Consultation cancelled", "The other participant cancelled the consultation.", url, data),
            ConsultationEventTypes.BookingExpired => One(payload.ClientUserId,
                "consultation.booking.expired", NotificationSeverity.Warning,
                "Consultation reservation expired", "Payment was not completed before the slot reservation expired.", url, data),
            ConsultationEventTypes.ConsultationPerformed => One(payload.ClientUserId,
                "consultation.performed", NotificationSeverity.Information,
                "Confirm your consultation", "The lawyer marked the consultation as performed. Confirm it or report a problem within 24 hours.", url, data),
            ConsultationEventTypes.ConsultationCompleted => One(payload.LawyerUserId,
                "consultation.completed", NotificationSeverity.Success,
                "Consultation completed", "The consultation was confirmed and its payment entered the release hold.", url, data),
            ConsultationEventTypes.ConsultationDisputed => One(payload.LawyerUserId,
                "consultation.disputed", NotificationSeverity.Warning,
                "Consultation payment frozen", "The client opened a consultation dispute. Funds remain frozen until review.", url, data),
            ConsultationEventTypes.DisputeSettled => Two(payload.ClientUserId, payload.LawyerUserId,
                "consultation.dispute.settled", NotificationSeverity.Information,
                "Consultation dispute settled", "An administrator completed the consultation payment settlement.", url, data),
            ConsultationEventTypes.PaymentReleased => One(payload.LawyerUserId,
                "consultation.payment.released", NotificationSeverity.Success,
                "Consultation payment released", "The consultation net amount is now available in your wallet.", url, data),
            _ => throw new InvalidOperationException($"Unsupported consultation event '{message.EventType}'.")
        };
        return Task.FromResult(drafts);
    }

    private static IReadOnlyCollection<NotificationDraft> One(
        Guid recipient, string type, NotificationSeverity severity,
        string title, string body, string url, IReadOnlyDictionary<string, string> data) =>
        [new(recipient, type, severity, title, body, url, data)];

    private static IReadOnlyCollection<NotificationDraft> Two(
        Guid first, Guid second, string type, NotificationSeverity severity,
        string title, string body, string url, IReadOnlyDictionary<string, string> data) =>
        [new(first, type, severity, title, body, url, data),
         new(second, type, severity, title, body, url, data)];

    private static Guid OtherParticipant(ConsultationEventPayload payload) =>
        payload.ActorUserId == payload.ClientUserId ? payload.LawyerUserId : payload.ClientUserId;
}
