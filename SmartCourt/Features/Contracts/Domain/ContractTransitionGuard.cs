using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Contracts.Enums;

namespace SmartCourt.Features.Contracts.Domain;

internal static class ContractTransitionGuard
{
    private static readonly HashSet<(ContractStatus From, ContractStatus To)>
        AllowedTransitions =
        [
            (ContractStatus.Draft, ContractStatus.Active),
            (ContractStatus.Draft, ContractStatus.Terminated),
            (ContractStatus.Active, ContractStatus.CompletedOnHold),
            (ContractStatus.Active, ContractStatus.SuspendedByDispute),
            (ContractStatus.Active, ContractStatus.Completed),
            (ContractStatus.Active, ContractStatus.Terminated),
            (ContractStatus.CompletedOnHold, ContractStatus.Active),
            (ContractStatus.CompletedOnHold, ContractStatus.Completed),
            (ContractStatus.CompletedOnHold, ContractStatus.SuspendedByDispute),
            (ContractStatus.CompletedOnHold, ContractStatus.Terminated),
            (ContractStatus.SuspendedByDispute, ContractStatus.Active),
            (ContractStatus.SuspendedByDispute, ContractStatus.CompletedOnHold),
            (ContractStatus.SuspendedByDispute, ContractStatus.Completed),
            (ContractStatus.SuspendedByDispute, ContractStatus.Terminated)
        ];

    internal static void EnsureCanTransition(
        ContractStatus current,
        ContractStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"لا يمكن تغيير حالة العقد من '{current}' إلى '{next}'.");
        }
    }
}
