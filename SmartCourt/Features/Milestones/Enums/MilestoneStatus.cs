namespace SmartCourt.Features.Milestones.Enums;

public enum MilestoneStatus : int
{
    Draft = 0,
    AwaitingFunding = 1,
    FundingProcessing = 2,
    FundedInProgress = 3,
    Submitted = 4,
    AcceptedHold = 5,
    Disputed = 6,
    Released = 7,
    Refunded = 8,
    Cancelled = 9,
    ReleasePending = 10
}
