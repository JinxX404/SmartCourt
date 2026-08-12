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

    internal PaymentWebhookEvent(
        Guid id,
        string providerCode,
        string eventId,
        string eventType,
        string? providerObjectId,
        string? connectedAccountId,
        Guid? paymentTransactionId,
        DateTime receivedAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ProviderCode = EntityGuard.Required(providerCode, nameof(providerCode));
        EventId = EntityGuard.Required(eventId, nameof(eventId));
        EventType = EntityGuard.Required(eventType, nameof(eventType));
        ProviderObjectId = providerObjectId;
        ConnectedAccountId = connectedAccountId;
        PaymentTransactionId = paymentTransactionId;
        ReceivedAt = EntityGuard.Utc(receivedAt, nameof(receivedAt));
    }

    public Guid Id { get; private set; }
    public string EventId { get; private set; } = string.Empty;
    public string ProviderCode { get; private set; } = "Legacy";
    public string EventType { get; private set; } = "legacy.payment";
    public string? ProviderObjectId { get; private set; }
    public string? ConnectedAccountId { get; private set; }
    public Guid? PaymentTransactionId { get; private set; }
    public DateTime ReceivedAt { get; private set; }
    public DateTime? ProcessedAt { get; internal set; }
    public string? ProcessingError { get; internal set; }
}
