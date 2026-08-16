namespace SmartCourt.Features.Ratings.DTOs;

public sealed record LawyerRatingsQuery(
    int Page = 1,
    int PageSize = 10);
