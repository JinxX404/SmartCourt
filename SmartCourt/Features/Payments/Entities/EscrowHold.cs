using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Payments.Enums;

namespace SmartCourt.Features.Payments.Entities;

public sealed class EscrowHold
{
    private EscrowHold()
    {
    }

    internal EscrowHold(
        Guid id,
        Guid escrowAccountId,
        Guid contractId,
        Guid milestoneId,
        decimal grossAmount,
        decimal platformFeeAmount,
        decimal netAmount,
        Guid providerDepositTransactionId,
        DateTime fundedAt,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        EscrowAccountId = EntityGuard.NotEmpty(
            escrowAccountId,
            nameof(escrowAccountId));
        ContractId = EntityGuard.NotEmpty(contractId, nameof(contractId));
        MilestoneId = EntityGuard.NotEmpty(milestoneId, nameof(milestoneId));
        GrossAmount = EntityGuard.PositiveMoney(grossAmount, nameof(grossAmount));
        PlatformFeeAmount = EntityGuard.NonNegativeMoney(
            platformFeeAmount,
            nameof(platformFeeAmount));
        NetAmount = EntityGuard.NonNegativeMoney(netAmount, nameof(netAmount));
        if (GrossAmount != PlatformFeeAmount + NetAmount)
        {
            throw new BusinessException(
                "Escrow hold net amount and platform fee must reconcile to gross.");
        }

        ProviderDepositTransactionId = EntityGuard.NotEmpty(
            providerDepositTransactionId,
            nameof(providerDepositTransactionId));
        Status = EscrowHoldStatus.Funded;
        FundedAt = EntityGuard.Utc(fundedAt, nameof(fundedAt));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid EscrowAccountId { get; internal set; }
    public Guid ContractId { get; internal set; }
    public Guid MilestoneId { get; internal set; }
    public decimal GrossAmount { get; internal set; }
    public decimal PlatformFeeAmount { get; internal set; }
    public decimal NetAmount { get; internal set; }
    public EscrowHoldStatus Status { get; internal set; }
    public DateTime FundedAt { get; internal set; }
    public DateTime? HoldStartsAt { get; internal set; }
    public DateTime? HoldExpiresAt { get; internal set; }
    public DateTime? FrozenAt { get; internal set; }
    public DateTime? SettledAt { get; internal set; }
    public SettlementType? SettlementType { get; internal set; }
    public Guid ProviderDepositTransactionId { get; internal set; }
    public Guid? ProviderReleaseTransactionId { get; internal set; }
    public Guid? ProviderRefundTransactionId { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
}
