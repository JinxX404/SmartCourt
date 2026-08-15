namespace SmartCourt.Features.Payments.FundingVerification;

public sealed record VerifiedMilestoneFunding(
    Guid MilestoneId,
    Guid ContractId,
    Guid EscrowAccountId,
    Guid EscrowHoldId,
    Guid DepositTransactionId,
    decimal GrossAmount,
    string Currency,
    DateTimeOffset FundedAt);
