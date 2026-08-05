namespace SmartCourt.Features.Contracts;

public interface IContractActivationEvaluator
{
    Task EvaluateActivationAsync(
        Guid contractId,
        Guid? actorUserId,
        CancellationToken cancellationToken);
}
