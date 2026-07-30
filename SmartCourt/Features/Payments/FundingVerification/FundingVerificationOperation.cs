namespace SmartCourt.Features.Payments.FundingVerification;

public enum FundingVerificationOperation : int
{
    Submission = 0,
    ManualAcceptance = 1,
    AutomaticAcceptance = 2,
    DisputeOpening = 3
}
