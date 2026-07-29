using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts;

public interface IContractService
{
    Task<ContractDetailDto> CreateAsync(
        CreateContractRequest request,
        CancellationToken cancellationToken);

    Task<PagedResult<ContractSummaryDto>> ListAsync(
        ContractListQuery query,
        CancellationToken cancellationToken);

    Task<ContractDetailDto> GetAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<ContractDetailDto> UpdateDraftAsync(
        Guid contractId,
        UpdateContractRequest request,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<ContractActionResultDto> AcceptAsync(
        Guid contractId,
        string ifMatch,
        CancellationToken cancellationToken);

    Task<ContractActionResultDto> EvaluateActivationAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<PagedResult<ContractStateHistoryDto>> GetStateHistoryAsync(
        Guid contractId,
        ContractStateHistoryQuery query,
        CancellationToken cancellationToken);

    Task<ContractActionResultDto> EvaluateCompletionAsync(
        Guid contractId,
        CancellationToken cancellationToken);

    Task<ContractDetailDto> TerminateAsync(
        Guid contractId,
        TerminateContractRequest request,
        string ifMatch,
        CancellationToken cancellationToken);
}
