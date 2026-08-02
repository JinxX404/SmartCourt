using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Infrastructure.Providers.Events;

public interface IOutboxWriter
{
    Task<OutboxMessage> EnqueueAsync(
        OutboxEvent @event,
        CancellationToken cancellationToken);
}
