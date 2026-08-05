namespace SmartCourt.Features.Payments.FundingVerification;

public interface IMilestoneFundingVerifier
{
    Task<VerifiedMilestoneFunding> VerifyAsync(
        Guid milestoneId,
        FundingVerificationOperation operation,
        CancellationToken cancellationToken);
}
