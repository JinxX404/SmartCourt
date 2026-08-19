using System.Collections.Generic;

namespace SmartCourt.Features.LawyerSubscription.DTOs;

public sealed record LawyerBundleDto(
    string Id,
    string Name,
    decimal CreditAmount,
    decimal PriceEgp
);

public sealed record LawyerBundleListDto(
    List<LawyerBundleDto> Bundles
);
