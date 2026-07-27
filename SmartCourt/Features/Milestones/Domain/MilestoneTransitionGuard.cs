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
            (MilestoneStatus.FundingProcessing, MilestoneStatus.AwaitingFunding),
            (MilestoneStatus.FundingProcessing, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Submitted),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Cancelled),
            (MilestoneStatus.FundedInProgress, MilestoneStatus.Refunded),
            (MilestoneStatus.Submitted, MilestoneStatus.FundedInProgress),
            (MilestoneStatus.Submitted, MilestoneStatus.AcceptedHold),
            (MilestoneStatus.Submitted, MilestoneStatus.Refunded),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Disputed),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Released),
            (MilestoneStatus.AcceptedHold, MilestoneStatus.Refunded),
            (MilestoneStatus.Disputed, MilestoneStatus.Released),
            (MilestoneStatus.Disputed, MilestoneStatus.Refunded)
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
                $"Invalid milestone transition from '{current}' to '{next}'.");
        }
    }
}
