using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Domain;

internal static class EscrowHoldTransitionGuard
{
    private static readonly HashSet<(
        EscrowHoldStatus From,
        EscrowHoldStatus To)> AllowedTransitions =
        [
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Frozen),
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Released),
            (EscrowHoldStatus.Funded, EscrowHoldStatus.Refunded),
            (EscrowHoldStatus.Frozen, EscrowHoldStatus.Funded),
            (EscrowHoldStatus.Frozen, EscrowHoldStatus.Released),
            (EscrowHoldStatus.Frozen, EscrowHoldStatus.Refunded)
        ];

    internal static void EnsureCanTransition(
        EscrowHoldStatus current,
        EscrowHoldStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"لا يمكن تغيير حالة حجز الضمان من {current} إلى {next}.");
        }
    }
}
