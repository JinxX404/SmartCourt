using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;

namespace SmartCourt.Features.Payments.Settlement;

internal static class SettlementCalculator
{
    private const decimal PlatformFeeRate = 0.05m;

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
        var platformFee = decimal.Round(
            lawyerGrossAllocation * PlatformFeeRate,
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
