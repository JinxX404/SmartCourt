using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Domain;

internal static class DisputeTransitionGuard
{
    private static readonly HashSet<(DisputeStatus From, DisputeStatus To)>
        AllowedTransitions =
        [
            (DisputeStatus.Open, DisputeStatus.Assigned),
            (DisputeStatus.Assigned, DisputeStatus.UnderReview),
            (DisputeStatus.UnderReview, DisputeStatus.Resolved),
            (DisputeStatus.Resolved, DisputeStatus.Closed)
        ];

    internal static void EnsureCanTransition(
        DisputeStatus current,
        DisputeStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"Invalid dispute transition from '{current}' to '{next}'.");
        }
    }
}
