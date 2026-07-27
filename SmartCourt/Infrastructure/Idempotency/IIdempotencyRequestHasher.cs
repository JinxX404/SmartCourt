namespace SmartCourt.Infrastructure.Idempotency;

public interface IIdempotencyRequestHasher
{
    string ComputeHash<TRequest>(
        IdempotencyScope scope,
        TRequest request);
}
