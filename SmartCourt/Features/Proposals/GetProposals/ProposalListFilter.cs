using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.GetProposals;

public sealed class ProposalListFilter
{
    [FromQuery(Name = "statuses")]
    public ProposalStatus[]? Statuses { get; init; }

    [FromQuery(Name = "search")]
    public string? Search { get; init; }

    [FromQuery(Name = "page")]
    public int Page { get; init; } = 1;

    [FromQuery(Name = "pageSize")]
    public int PageSize { get; init; } = 5;
}
