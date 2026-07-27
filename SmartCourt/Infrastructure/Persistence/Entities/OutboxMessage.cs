using SmartCourt.Common.Domain;
using SmartCourt.Infrastructure.Persistence.Enums;

namespace SmartCourt.Infrastructure.Persistence.Entities;

public sealed class OutboxMessage
{
    private OutboxMessage()
    {
    }

    internal OutboxMessage(
        Guid id,
        string eventType,
        int eventVersion,
        string payload,
        string aggregateType,
        Guid aggregateId,
        Guid correlationId,
        DateTime availableAt,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        EventType = EntityGuard.Required(eventType, nameof(eventType));
        EventVersion = EntityGuard.Positive(eventVersion, nameof(eventVersion));
        Payload = EntityGuard.Required(payload, nameof(payload));
        AggregateType = EntityGuard.Required(aggregateType, nameof(aggregateType));
        AggregateId = EntityGuard.NotEmpty(aggregateId, nameof(aggregateId));
        CorrelationId = EntityGuard.NotEmpty(correlationId, nameof(correlationId));
        Status = OutboxStatus.Pending;
        AvailableAt = EntityGuard.Utc(availableAt, nameof(availableAt));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; internal set; }
    public string EventType { get; internal set; } = string.Empty;
    public int EventVersion { get; internal set; }
    public string Payload { get; internal set; } = string.Empty;
    public string AggregateType { get; internal set; } = string.Empty;
    public Guid AggregateId { get; internal set; }
    public Guid CorrelationId { get; internal set; }
    public OutboxStatus Status { get; internal set; }
    public int Attempts { get; internal set; }
    public string? LastError { get; internal set; }
    public DateTime AvailableAt { get; internal set; }
    public DateTime? ProcessedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
}
