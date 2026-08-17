namespace SmartCourt.Features.Notifications.Events;

internal static class NotificationActionUrls
{
    public static string Consultation(Guid bookingId) =>
        $"/dashboard?tab=consultations&bookingId={bookingId}";

    public static string Proposal(Guid proposalId) =>
        $"/dashboard/proposals/{proposalId}";

    public static string Contract(Guid contractId) =>
        $"/dashboard/contracts/{contractId}";

    public static string Milestone(Guid contractId, Guid milestoneId) =>
        $"/dashboard/contracts/{contractId}?milestoneId={milestoneId}";

    public static string Wallet => "/dashboard/wallet";

    public static string Verification => "/dashboard?tab=verification";

    public static string VerificationReview(Guid userId) =>
        $"/dashboard?tab=admin-verifications&userId={userId}";

    public static string Security => "/dashboard?tab=settings";

    public static string Article(Guid articleId) => $"/articles/{articleId}";

    public static string ArticleListing => "/dashboard?tab=articles";

    public static string ArticleReport(Guid articleId) =>
        $"/dashboard?tab=admin-articles&section=reports&articleId={articleId}";
}
