namespace SmartCourt.Features.Payments.FundingVerification;

internal interface IMilestoneFundingVerifier
{
    Task<VerifiedMilestoneFunding> VerifyAsync(
        Guid milestoneId,
        FundingVerificationOperation operation,
        CancellationToken cancellationToken);
}
