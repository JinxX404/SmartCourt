using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
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
        DateTimeOffset availableAt,
        DateTimeOffset createdAt)
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
    public DateTimeOffset AvailableAt { get; internal set; }
    public Guid? LeaseId { get; internal set; }
    public DateTimeOffset? LeaseExpiresAt { get; internal set; }
    public DateTimeOffset? ProcessedAt { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTimeOffset CreatedAt { get; internal set; }

    internal void Claim(
        Guid leaseId,
        DateTimeOffset nowUtc,
        TimeSpan leaseDuration)
    {
        Id = EntityGuard.NotEmpty(Id, nameof(Id));
        LeaseId = EntityGuard.NotEmpty(leaseId, nameof(leaseId));
        nowUtc = EntityGuard.Utc(nowUtc, nameof(nowUtc));
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new BusinessException(
                "يجب أن تكون مدة حجز رسالة صندوق الصادر أكبر من صفر.");
        }

        Status = OutboxStatus.Processing;
        Attempts++;
        LeaseExpiresAt = nowUtc.Add(leaseDuration);
        LastError = null;
    }

    internal void MarkProcessed(
        Guid leaseId,
        DateTimeOffset processedAtUtc)
    {
        EnsureLease(leaseId);
        Status = OutboxStatus.Processed;
        ProcessedAt = EntityGuard.Utc(
            processedAtUtc,
            nameof(processedAtUtc));
        LeaseId = null;
        LeaseExpiresAt = null;
    }

    internal void MarkFailed(
        Guid leaseId,
        string error,
        DateTimeOffset availableAtUtc)
    {
        EnsureLease(leaseId);
        Status = OutboxStatus.Failed;
        LastError = EntityGuard.Required(error, nameof(error));
        if (LastError.Length > 2_000)
        {
            LastError = LastError[..2_000];
        }

        AvailableAt = EntityGuard.Utc(
            availableAtUtc,
            nameof(availableAtUtc));
        LeaseId = null;
        LeaseExpiresAt = null;
    }

    private void EnsureLease(Guid leaseId)
    {
        if (Status != OutboxStatus.Processing
            || LeaseId != leaseId)
        {
            throw new BusinessException(
                "انتهت صلاحية حجز رسالة صندوق الصادر أو لم يعد صالحًا.");
        }
    }
}
