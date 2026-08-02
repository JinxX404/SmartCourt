using SmartCourt.Features.Contracts.DTOs;

namespace SmartCourt.Features.Contracts;

public interface IContractCompletionEvaluator
{
    Task<ContractActionResultDto> EvaluateCompletionAsync(
        Guid contractId,
        CancellationToken cancellationToken);
}
