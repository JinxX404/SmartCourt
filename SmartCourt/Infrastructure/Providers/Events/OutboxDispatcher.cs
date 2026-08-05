using System.Data;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Persistence.Enums;
using SmartCourt.Persistence;

namespace SmartCourt.Infrastructure.Providers.Events;

public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private static readonly TimeSpan LeaseDuration =
        TimeSpan.FromMinutes(5);
    private const int MaximumErrorLength = 2_000;

    private readonly ApplicationDbContext _dbContext;
    private readonly IReadOnlyCollection<IOutboxEventHandler> _handlers;
    private readonly TimeProvider _timeProvider;

    public OutboxDispatcher(
        ApplicationDbContext dbContext,
        IEnumerable<IOutboxEventHandler> handlers,
        TimeProvider timeProvider)
    {
        _dbContext = dbContext;
        _handlers = handlers.ToArray();
        _timeProvider = timeProvider;
    }

    public async Task<int> DispatchAvailableAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(batchSize));
        }

        var processed = 0;
        while (processed < batchSize)
        {
            var lease = await ClaimAsync(
                messageId: null,
                cancellationToken);
            if (lease is null)
            {
                break;
            }

            if (await DispatchClaimedAsync(lease, cancellationToken))
            {
                processed++;
            }
        }

        return processed;
    }

    public async Task DispatchAsync(
        Guid outboxMessageId,
        CancellationToken cancellationToken)
    {
        var lease = await ClaimAsync(
            outboxMessageId,
            cancellationToken);
        if (lease is not null)
        {
            await DispatchClaimedAsync(lease, cancellationToken);
        }
    }

    private async Task<bool> DispatchClaimedAsync(
        OutboxLease lease,
        CancellationToken cancellationToken)
    {
        try
        {
            var handlers = _handlers
                .Where(handler => handler.EventTypes.Contains(
                    lease.EventType,
                    StringComparer.Ordinal))
                .ToArray();
            if (handlers.Length == 0)
            {
                throw new InvalidOperationException(
                    $"No outbox handler is registered for {lease.EventType}.");
            }

            foreach (var handler in handlers)
            {
                await handler.HandleAsync(
                    lease.Message,
                    cancellationToken);
            }

            await _dbContext.Database
                .CreateExecutionStrategy()
                .ExecuteAsync(async () =>
                {
                    lease.Message.MarkProcessed(
                        lease.LeaseId,
                        _timeProvider.GetUtcNow().UtcDateTime);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    return true;
                });
            return true;
        }
        catch (Exception exception) when (
            exception is not OperationCanceledException)
        {
            var error = exception.Message;
            if (error.Length > MaximumErrorLength)
            {
                error = error[..MaximumErrorLength];
            }

            var now = _timeProvider.GetUtcNow().UtcDateTime;
            var delaySeconds = Math.Min(
                3_600,
                Math.Pow(2, Math.Min(lease.Message.Attempts - 1, 10)));
            lease.Message.MarkFailed(
                lease.LeaseId,
                error,
                now.AddSeconds(delaySeconds));
            await _dbContext.SaveChangesAsync(cancellationToken);
            return false;
        }
    }

    private async Task<OutboxLease?> ClaimAsync(
        Guid? messageId,
        CancellationToken cancellationToken)
    {
        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);
        var now = _timeProvider.GetUtcNow().UtcDateTime;
        var query = _dbContext.OutboxMessages
            .Where(message =>
                (message.Status == OutboxStatus.Pending
                    || message.Status == OutboxStatus.Failed
                    || message.Status == OutboxStatus.Processing
                    && message.LeaseExpiresAt <= now)
                && message.AvailableAt <= now);
        if (messageId.HasValue)
        {
            query = query.Where(message => message.Id == messageId.Value);
        }

        var message = await query
            .OrderBy(item => item.AvailableAt)
            .ThenBy(item => item.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
        if (message is null)
        {
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var leaseId = Guid.NewGuid();
        message.Claim(leaseId, now, LeaseDuration);
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new OutboxLease(
                message,
                leaseId,
                message.EventType);
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync(cancellationToken);
            _dbContext.ChangeTracker.Clear();
            return null;
        }
    }

    private sealed record OutboxLease(
        OutboxMessage Message,
        Guid LeaseId,
        string EventType);
}
