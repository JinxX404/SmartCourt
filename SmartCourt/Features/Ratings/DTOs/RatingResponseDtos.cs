using SmartCourt.Features.Ratings.Enums;

namespace SmartCourt.Features.Ratings.DTOs;

public sealed record ContractRatingDto(
    Guid Id,
    Guid ContractId,
    Guid RaterUserId,
    Guid RatedUserId,
    RaterRole RaterRole,
    int Stars,
    string? Comment,
    DateTime CreatedAt);

public sealed record ContractRatingSummaryDto(
    Guid ContractId,
    bool AreRevealed,
    ContractRatingDto? ClientRating,
    ContractRatingDto? LawyerRating);
