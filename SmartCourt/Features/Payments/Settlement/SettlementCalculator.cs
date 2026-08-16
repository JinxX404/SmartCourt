using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Payments;

namespace SmartCourt.Features.Payments.Settlement;

internal static class SettlementCalculator
{
    internal static SettlementBreakdown Calculate(
        decimal grossAmount,
        decimal clientRefundAmount)
    {
        var gross = EntityGuard.PositiveMoney(
            grossAmount,
            nameof(grossAmount));
        var refund = EntityGuard.NonNegativeMoney(
            clientRefundAmount,
            nameof(clientRefundAmount));

        if (refund > gross)
        {
            throw new BusinessException(
                "لا يمكن أن يتجاوز مبلغ رد العميل إجمالي مبلغ التسوية.");
        }

        var lawyerGrossAllocation = gross - refund;
        var platformFee = PlatformFeePolicy.Calculate(lawyerGrossAllocation);
        var lawyerNet = lawyerGrossAllocation - platformFee;

        return new SettlementBreakdown(
            gross,
            refund,
            lawyerGrossAllocation,
            platformFee,
            lawyerNet);
    }

    internal static SettlementBreakdown CalculateFromFundedHold(
        decimal grossAmount,
        decimal snapshottedPlatformFeeAmount,
        decimal clientRefundAmount)
    {
        var gross = EntityGuard.PositiveMoney(grossAmount, nameof(grossAmount));
        var snapshottedFee = EntityGuard.NonNegativeMoney(
            snapshottedPlatformFeeAmount,
            nameof(snapshottedPlatformFeeAmount));
        var refund = EntityGuard.NonNegativeMoney(
            clientRefundAmount,
            nameof(clientRefundAmount));

        if (refund > gross || snapshottedFee > gross)
        {
            throw new BusinessException(
                "بيانات حجز الضمان المحفوظة غير صالحة للتسوية.");
        }

        var lawyerGrossAllocation = gross - refund;
        var platformFee = decimal.Round(
            snapshottedFee * lawyerGrossAllocation / gross,
            2,
            MidpointRounding.AwayFromZero);
        var lawyerNet = lawyerGrossAllocation - platformFee;

        return new SettlementBreakdown(
            gross,
            refund,
            lawyerGrossAllocation,
            platformFee,
            lawyerNet);
    }
}
