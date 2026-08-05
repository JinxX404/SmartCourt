namespace SmartCourt.Infrastructure.Providers.Events;

public sealed record OutboxEvent(
    string EventType,
    int EventVersion,
    object Payload,
    string AggregateType,
    Guid AggregateId,
    Guid CorrelationId,
    DateTime? AvailableAtUtc = null);
