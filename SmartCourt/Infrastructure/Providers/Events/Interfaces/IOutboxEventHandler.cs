using SmartCourt.Infrastructure.Persistence.Entities;

namespace SmartCourt.Infrastructure.Providers.Events;

public interface IOutboxEventHandler
{
    IReadOnlyCollection<string> EventTypes { get; }

    Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken);
}
