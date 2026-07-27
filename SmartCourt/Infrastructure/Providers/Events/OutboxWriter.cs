using System.Text.Json;
using SmartCourt.Common.Exceptions;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Persistence;

namespace SmartCourt.Infrastructure.Providers.Events;

public sealed class OutboxWriter : IOutboxWriter
{
    private const int MaximumPayloadLength = 20_000;
    private static readonly JsonSerializerOptions SerializerOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ApplicationDbContext _dbContext;
    private readonly TimeProvider _timeProvider;

    public OutboxWriter(
        ApplicationDbContext dbContext,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _timeProvider = timeProvider;
    }

    public async Task<OutboxMessage> EnqueueAsync(
        OutboxEvent @event,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ValidateEvent(@event);

        var payload = JsonSerializer.Serialize(
            @event.Payload,
            SerializerOptions);
        if (payload.Length > MaximumPayloadLength)
        {
            throw new BusinessException(
                "Outbox payload exceeds the maximum allowed size.");
        }

        OutboxPayloadPolicy.Validate(payload);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var message = new OutboxMessage(
            Guid.NewGuid(),
            @event.EventType,
            @event.EventVersion,
            payload,
            @event.AggregateType,
            @event.AggregateId,
            @event.CorrelationId,
            @event.AvailableAtUtc ?? now,
            now);
        _dbContext.OutboxMessages.Add(message);
        await Task.CompletedTask;
        return message;
    }

    private static void ValidateEvent(OutboxEvent @event)
    {
        if (@event.Payload is null)
        {
            throw new BusinessException("Outbox payload is required.");
        }

        if (@event.EventVersion <= 0)
        {
            throw new BusinessException(
                "Outbox event version must be positive.");
        }

        if (@event.AggregateId == Guid.Empty
            || @event.CorrelationId == Guid.Empty)
        {
            throw new BusinessException(
                "Outbox aggregate and correlation IDs are required.");
        }
    }
}
