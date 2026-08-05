using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.DTOs;

public sealed record ContractListQuery(
    ContractStatus? Status = null,
    int Page = 1,
    int PageSize = 10);

public sealed record ContractStateHistoryQuery(
    int Page = 1,
    int PageSize = 100);

