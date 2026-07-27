namespace SmartCourt.Infrastructure.Providers.Events;

public interface IOutboxDispatcher
{
    Task<int> DispatchAvailableAsync(
        int batchSize,
        CancellationToken cancellationToken);

    Task DispatchAsync(
        Guid outboxMessageId,
        CancellationToken cancellationToken);
}
