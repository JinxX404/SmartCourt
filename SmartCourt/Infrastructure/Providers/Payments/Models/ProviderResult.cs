namespace SmartCourt.Infrastructure.Providers.Payments;

public sealed record ProviderResult(
    decimal Amount,
    string Currency,
    Guid BusinessId,
    string ProviderIdempotencyKey,
    Guid CorrelationId,
    ProviderOperationOutcome Outcome,
    string? ProviderTransactionId,
    string? FailureReason,
    string? ProviderStatus = null,
    string? ProviderObjectType = null,
    ProviderMoney? ProviderMoney = null,
    ProviderClientAction? ClientAction = null,
    string? RelatedProviderTransactionId = null);

public sealed record ProviderMoney(
    long AmountMinor,
    string Currency);

public sealed record ProviderClientAction(
    ProviderClientActionType Type,
    string? ClientSecret = null,
    string? RedirectUrl = null);

public enum ProviderClientActionType
{
    ConfirmPayment = 0,
    Redirect = 1
}
