namespace SmartCourt.Features.ChatAgent.Monetization;

public interface ITokenBundleFulfillmentService
{
    Task ReconcileProviderObjectAsync(
        string providerObjectId,
        CancellationToken cancellationToken);
}
