using SmartCourt.Common.Enums;
using SmartCourt.Common.Models;

namespace SmartCourt.Features.Users.Lawyers.DTOs;

public class SearchLawyersRequest : PagedRequest
{
    public string? SearchTerm { get; set; }
    public string? Governorate { get; set; }
    public LawyerLevel? Level { get; set; }
    public Specialization? Specialization { get; set; }
    public decimal? MinRating { get; set; }
    public bool? IsAvailable { get; set; }
    public LawyerSortBy SortBy { get; set; } = LawyerSortBy.Rating;
    public SortDirection SortDirection { get; set; } = SortDirection.Descending;
}
