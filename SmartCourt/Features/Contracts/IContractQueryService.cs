using SmartCourt.Features.Contracts.DTOs;
using SmartCourt.Features.Contracts.Entities;

namespace SmartCourt.Features.Contracts;

public interface IContractQueryService
{
    Task<PagedResult<ContractSummaryDto>> ListAsync(
        ContractListQuery query,
        CancellationToken cancellationToken);

    Task<ContractDetailDto> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<PagedResult<ContractStateHistoryDto>> GetStateHistoryAsync(
        Guid contractId,
        ContractStateHistoryQuery query,
        CancellationToken cancellationToken);

    Task<ContractDetailDto> MapDetailAsync(
        Contract contract,
        CancellationToken cancellationToken);
}
