using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Auth.Events;

internal static class AuthOutbox
{
    public static Task EnqueuePasswordChangedAsync(
        IOutboxWriter outboxWriter,
        Guid userId,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        EnqueueAsync(
            outboxWriter,
            AuthEventTypes.PasswordChanged,
            userId,
            correlationId,
            cancellationToken);

    public static Task EnqueuePasswordResetAsync(
        IOutboxWriter outboxWriter,
        Guid userId,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        EnqueueAsync(
            outboxWriter,
            AuthEventTypes.PasswordReset,
            userId,
            correlationId,
            cancellationToken);

    private static Task EnqueueAsync(
        IOutboxWriter outboxWriter,
        string eventType,
        Guid userId,
        Guid correlationId,
        CancellationToken cancellationToken) =>
        outboxWriter.EnqueueAsync(
            new OutboxEvent(
                eventType,
                1,
                new AuthPasswordSecurityEventPayload(userId),
                nameof(ApplicationUser),
                userId,
                correlationId),
            cancellationToken);
}
