using SmartCourt.Features.LawyerSubscription.DTOs;
using System.Threading;
using System.Threading.Tasks;
using SmartCourt.Features.LawyerSubscription.Enums;

namespace SmartCourt.Features.LawyerSubscription;

public interface ILawyerSubscriptionPaymentService
{
    Task<LawyerPaymentCheckoutResponse> PurchaseBundleAsync(string bundleId, string confirmationTokenReference, string? idempotencyKey, CancellationToken cancellationToken = default);
    Task<LawyerPaymentCheckoutResponse> PurchaseSubscriptionAsync(LawyerPlanType newPlan, string confirmationTokenReference, string? idempotencyKey, CancellationToken cancellationToken = default);
    
    // For Webhooks
    Task ReconcileProviderObjectAsync(string providerObjectId, CancellationToken cancellationToken = default);
}

public sealed record LawyerPaymentCheckoutResponse(
    string TransactionId,
    string TargetId,
    string TargetType,
    decimal PriceEgp,
    string ClientSecret,
    string? RedirectUrl
);
