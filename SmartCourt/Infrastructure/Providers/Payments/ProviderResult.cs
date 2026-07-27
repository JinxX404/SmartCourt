namespace SmartCourt.Infrastructure.Providers.Payments;

public sealed record ProviderResult(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    ProviderOperationOutcome Outcome,
    string? ProviderTransactionId,
    string? FailureReason);
