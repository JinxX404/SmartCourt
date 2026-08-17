using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Domain;

internal static class DisputeTransitionGuard
{
    private static readonly HashSet<(DisputeStatus From, DisputeStatus To)>
        AllowedTransitions =
        [
            (DisputeStatus.Open, DisputeStatus.Assigned),
            (DisputeStatus.Open, DisputeStatus.Cancelled),
            (DisputeStatus.Assigned, DisputeStatus.UnderReview),
            (DisputeStatus.Assigned, DisputeStatus.Assigned),
            (DisputeStatus.Assigned, DisputeStatus.Cancelled),
            (DisputeStatus.UnderReview, DisputeStatus.Resolved),
            (DisputeStatus.UnderReview, DisputeStatus.Cancelled),
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
                $"لا يمكن تغيير حالة النزاع من '{current}' إلى '{next}'.");
        }
    }
}
