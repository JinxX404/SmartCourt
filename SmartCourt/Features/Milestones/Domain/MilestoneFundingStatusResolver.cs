using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Milestones.Enums;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Milestones.Domain;

public static class MilestoneFundingStatusResolver
{
    public static MilestoneFundingStatus Resolve(
        MilestoneStatus milestoneStatus,
        EscrowHold? hold)
    {
        return milestoneStatus switch
        {
            MilestoneStatus.FundingProcessing =>
                MilestoneFundingStatus.Processing,
            MilestoneStatus.Released or MilestoneStatus.Refunded =>
                MilestoneFundingStatus.Settled,
            _ when hold?.Status is EscrowHoldStatus.Released
                or EscrowHoldStatus.Refunded =>
                MilestoneFundingStatus.Settled,
            _ when hold is not null =>
                MilestoneFundingStatus.Funded,
            _ => MilestoneFundingStatus.Unfunded
        };
    }
}
