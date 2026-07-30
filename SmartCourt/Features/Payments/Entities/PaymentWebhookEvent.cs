using SmartCourt.Common.Domain;

namespace SmartCourt.Features.Payments.Entities;

public sealed class PaymentWebhookEvent
{
    private PaymentWebhookEvent()
    {
    }

    internal PaymentWebhookEvent(
        Guid id,
        string eventId,
        Guid paymentTransactionId,
        DateTime receivedAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        EventId = EntityGuard.Required(eventId, nameof(eventId));
        PaymentTransactionId = EntityGuard.NotEmpty(
            paymentTransactionId,
            nameof(paymentTransactionId));
        ReceivedAt = EntityGuard.Utc(receivedAt, nameof(receivedAt));
    }

    public Guid Id { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    public Guid PaymentTransactionId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
}
