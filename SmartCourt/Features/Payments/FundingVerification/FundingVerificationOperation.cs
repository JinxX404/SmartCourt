namespace SmartCourt.Features.Payments.FundingVerification;

internal enum FundingVerificationOperation : int
{
    Submission = 0,
    ManualAcceptance = 1,
    AutomaticAcceptance = 2,
    DisputeOpening = 3
}
