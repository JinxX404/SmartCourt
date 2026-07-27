namespace SmartCourt.Features.Payments.FundingVerification;

internal sealed record VerifiedMilestoneFunding(
    Guid MilestoneId,
    Guid ContractId,
    Guid EscrowAccountId,
    Guid EscrowHoldId,
    Guid DepositTransactionId,
    decimal GrossAmount,
    string Currency,
    DateTime FundedAt);
