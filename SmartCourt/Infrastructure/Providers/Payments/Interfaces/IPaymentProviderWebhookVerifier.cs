namespace SmartCourt.Infrastructure.Providers.Payments;

public interface IPaymentProviderWebhookVerifier
{
    ProviderWebhookEvent Verify(
        string rawBody,
        string signature,
        ProviderWebhookScope scope);
}

public enum ProviderWebhookScope
{
    Platform = 0,
    ConnectedAccounts = 1
}

public sealed record ProviderWebhookEvent(
    string EventId,
    string EventType,
    string? ProviderObjectId,
    string? ConnectedAccountId,
    bool IsLive);
