namespace SmartCourt.Infrastructure.Idempotency;

public interface IIdempotencyService
{
    Task<IdempotencyReservation> ReserveAsync<TRequest>(
        IdempotencyScope scope,
        string? idempotencyKey,
        TRequest request,
        CancellationToken cancellationToken);

    Task CompleteAsync<TResponse>(
        Guid recordId,
        int responseStatusCode,
        TResponse response,
        Guid? resultReferenceId,
        CancellationToken cancellationToken);

    Task FailAsync<TResponse>(
        Guid recordId,
        int responseStatusCode,
        TResponse response,
        Guid? resultReferenceId,
        CancellationToken cancellationToken);

    Task<int> PurgeExpiredResponseBodiesAsync(
        CancellationToken cancellationToken);
}
