using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Disputes.Enums;

namespace SmartCourt.Features.Disputes.Entities;

public sealed class DisputeResolution
{
    private DisputeResolution()
    {
    }

    internal DisputeResolution(
        Guid id,
        Guid disputeId,
        DisputeResolutionType resolutionType,
        decimal grossHoldAmount,
        decimal clientRefundAmount,
        decimal lawyerReleaseAmount,
        decimal platformFeeAmount,
        string summary,
        Guid resolvedByUserId,
        DateTime resolvedAt,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        DisputeId = EntityGuard.NotEmpty(disputeId, nameof(disputeId));
        ResolutionType = resolutionType;
        GrossHoldAmount = EntityGuard.PositiveMoney(
            grossHoldAmount,
            nameof(grossHoldAmount));
        ClientRefundAmount = EntityGuard.NonNegativeMoney(
            clientRefundAmount,
            nameof(clientRefundAmount));
        LawyerReleaseAmount = EntityGuard.NonNegativeMoney(
            lawyerReleaseAmount,
            nameof(lawyerReleaseAmount));
        PlatformFeeAmount = EntityGuard.NonNegativeMoney(
            platformFeeAmount,
            nameof(platformFeeAmount));
        if (GrossHoldAmount
            != ClientRefundAmount + LawyerReleaseAmount + PlatformFeeAmount)
        {
            throw new BusinessException(
                "يجب أن يساوي مجموع مبلغ رد العميل وصافي مستحق المحامي ورسوم المنصة إجمالي مبلغ الحجز.");
        }

        Summary = EntityGuard.Required(summary, nameof(summary));
        ResolvedByUserId = EntityGuard.NotEmpty(
            resolvedByUserId,
            nameof(resolvedByUserId));
        ResolvedAt = EntityGuard.Utc(resolvedAt, nameof(resolvedAt));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
    }

    public Guid Id { get; private set; }
    public Guid DisputeId { get; private set; }
    public DisputeResolutionType ResolutionType { get; private set; }
    public decimal GrossHoldAmount { get; private set; }
    public decimal ClientRefundAmount { get; private set; }
    public decimal LawyerReleaseAmount { get; private set; }
    public decimal PlatformFeeAmount { get; private set; }
    public string Summary { get; private set; } = string.Empty;
    public Guid ResolvedByUserId { get; private set; }
    public DateTime ResolvedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
}
