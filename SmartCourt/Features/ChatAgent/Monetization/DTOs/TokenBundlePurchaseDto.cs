using System;

namespace SmartCourt.Features.ChatAgent.Monetization.DTOs;

public sealed record TokenBundlePurchaseDto(
    Guid Id,
    string BundleId,
    decimal PriceEgp,
    decimal CreditAmount,
    string Status,
    string? FailureReason,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt
);

public sealed record TokenBundlePurchaseListDto(
    List<TokenBundlePurchaseDto> Purchases,
    int TotalCount
);
