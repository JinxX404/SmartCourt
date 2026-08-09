using SmartCourt.Features.Proposals.Enums;

namespace SmartCourt.Features.Proposals.Shared;

public static class ProposalPolicy
{
    public const int ActiveProposalLimitPerCase = 5;

    public static bool IsActive(ProposalStatus status)
    {
        return status is ProposalStatus.Pending or ProposalStatus.Accepted;
    }
}
