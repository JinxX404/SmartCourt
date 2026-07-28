using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Domain;

internal static class ChangeRequestTransitionGuard
{
    private static readonly HashSet<(
        ChangeRequestStatus From,
        ChangeRequestStatus To)> AllowedTransitions =
        [
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Approved),
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Rejected),
            (ChangeRequestStatus.Pending, ChangeRequestStatus.Cancelled)
        ];

    internal static void EnsureCanTransition(
        ChangeRequestStatus current,
        ChangeRequestStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"لا يمكن تغيير حالة طلب تعديل المرحلة من '{current}' إلى '{next}'.");
        }
    }
}
