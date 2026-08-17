using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Milestones.Enums;

namespace SmartCourt.Features.Milestones.Domain;

internal static class MilestoneTransitionGuard
{
    private static readonly HashSet<(MilestoneStatus From, MilestoneStatus To)>
        AllowedTransitions =
        [
            (MilestoneStatus.Draft, MilestoneStatus.AwaitingFunding),
            (MilestoneStatus.Draft, MilestoneStatus.Cancelled),
            (MilestoneStatus.AwaitingFunding, MilestoneStatus.FundingProcessing),
            (MilestoneStatus.AwaitingFunding, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.ReleasePending),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.AwaitingFunding),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Submitted),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Refunded),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Disputed),
            (MilestoneStatus.Submitted, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.Submitted, MilestoneStatus.AcceptedHold),
            (MilestoneStatus.Submitted, MilestoneStatus.Refunded),
            (MilestoneStatus.Submitted, MilestoneStatus.Disputed),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Disputed),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Released),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Refunded),
            (MilestoneStatus.Disputed, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.Disputed, MilestoneStatus.Submitted),
            (MilestoneStatus.Disputed, MilestoneStatus.AcceptedHold),
            (MilestoneStatus.Disputed, MilestoneStatus.Released),
            (MilestoneStatus.Disputed, MilestoneStatus.Refunded),
            (MilestoneStatus.ReleasePending, MilestoneStatus.Released)
        ];

    internal static void EnsureCanTransition(
        MilestoneStatus current,
        MilestoneStatus next)
    {
        if (!Enum.IsDefined(current)
            || !Enum.IsDefined(next)
            || !AllowedTransitions.Contains((current, next)))
        {
            throw new BusinessException(
                $"لا يمكن تغيير حالة المرحلة من '{current}' إلى '{next}'.");
        }
    }
}
