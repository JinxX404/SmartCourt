namespace SmartCourt.Features.Ratings.DTOs;

public sealed record SubmitRatingRequest(
    int Stars,
    string? Comment = null);

public sealed record UpdateRatingRequest(
    int Stars,
    string? Comment = null);

