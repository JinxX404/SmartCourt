using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts;

public sealed class ContractQueryService(
    IContractService contractService) : IContractQueryService
{
    public async Task<PagedResult<ContractSummaryDto>> ListAsync(
        ContractListQuery query,
        CancellationToken cancellationToken)
    {
        return await contractService.ListAsync(
            query,
            cancellationToken);
    }

    public async Task<ContractDetailDto> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken)
    {
        return await contractService.GetAsync(
            contractId,
            cancellationToken);
    }

    public async Task<PagedResult<ContractStateHistoryDto>> GetStateHistoryAsync(
        Guid contractId,
        ContractStateHistoryQuery query,
        CancellationToken cancellationToken)
    {
        return await contractService.GetStateHistoryAsync(
            contractId,
            query,
            cancellationToken);
    }
}
