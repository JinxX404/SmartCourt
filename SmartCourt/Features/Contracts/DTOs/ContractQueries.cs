using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.DTOs;

public sealed record ContractListQuery(
    ContractStatus? Status = null,
    int Page = 1,
    int PageSize = 10);

public sealed record ContractStateHistoryQuery(
    int Page = 1,
    int PageSize = 100);

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);
