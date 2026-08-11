using SmartCourt.Common.Enums;
using SmartCourt.Entities;
using SmartCourt.Features.Auth.Enums;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Admin.Verifications.Events;

internal static class VerificationOutbox
{
    public static Task EnqueueDocumentAsync(
        IOutboxWriter outboxWriter,
        string eventType,
        UserVerificationDocument document,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new VerificationDocumentEventPayload(
                    document.Id,
                    document.UserId,
                    document.DocumentType.ToString(),
                    document.Status.ToString()),
                nameof(UserVerificationDocument),
                document.Id,
                correlationId),
            cancellationToken);

    public static Task EnqueueAccountAsync(
        IOutboxWriter outboxWriter,
        string eventType,
        ApplicationUser user,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new VerificationAccountEventPayload(
                    user.Id,
                    user.Status.ToString()),
                nameof(ApplicationUser),
                user.Id,
                correlationId),
            cancellationToken);

    public static Task EnqueueReviewRequestedAsync(
        IOutboxWriter outboxWriter,
        ApplicationUser user,
        int documentCount,
        Guid correlationId,
        CancellationToken cancellationToken)
    {
        if (documentCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(documentCount));
        }

        return outboxWriter.EnqueueAsync(
            new OutboxEvent(
                VerificationEventTypes.ReviewRequested,
                1,
                new VerificationReviewRequestedEventPayload(
                    user.Id,
                    documentCount),
                nameof(ApplicationUser),
                user.Id,
                correlationId),
            cancellationToken);
    }
}
