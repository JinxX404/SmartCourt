namespace SmartCourt.Features.Payments.Integration;

public interface IContractTerminationSettlementService
{
    Task<ContractTerminationSettlement> SettleForTerminationAsync(
        Guid contractId,
        Guid actorUserId,
        string reason,
        Guid correlationId,
        CancellationToken cancellationToken);
}

public sealed record ContractTerminationSettlement(
    bool Completed,
    decimal GrossAmount,
    decimal ClientRefundAmount,
    decimal LawyerReleaseAmount,
    decimal PlatformFeeAmount);
