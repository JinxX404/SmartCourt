namespace SmartCourt.Features.Payments.Settlement;

internal sealed record SettlementBreakdown(
    decimal GrossAmount,
    decimal ClientRefundAmount,
    decimal LawyerGrossAllocation,
    decimal PlatformFeeAmount,
    decimal LawyerNetAmount);
