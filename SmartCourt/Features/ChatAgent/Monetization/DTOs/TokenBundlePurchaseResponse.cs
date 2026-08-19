namespace SmartCourt.Features.ChatAgent.Monetization.DTOs;

public sealed record TokenBundlePurchaseResponse(
    Guid TransactionId,
    string BundleId,
    string BundleName,
    decimal CreditAmount,
    decimal PriceEgp,
    string ClientSecret,
    string? RedirectUrl);
