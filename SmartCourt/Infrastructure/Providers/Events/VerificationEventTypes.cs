namespace SmartCourt.Infrastructure.Providers.Events;

public static class VerificationEventTypes
{
    public const string DocumentApproved = "VerificationDocumentApproved";
    public const string DocumentRejected = "VerificationDocumentRejected";
    public const string DocumentExpired = "VerificationDocumentExpired";
    public const string AccountApproved = "VerificationAccountApproved";
    public const string AccountRejected = "VerificationAccountRejected";
    public const string ReviewRequested = "VerificationReviewRequested";
}
