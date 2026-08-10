using SmartCourt.Features.Notifications.Enums;

namespace SmartCourt.Features.Notifications.Events;

internal sealed record NotificationDraft(
    Guid RecipientUserId,
    string Type,
    NotificationSeverity Severity,
    string Title,
    string Body,
    string? ActionUrl,
    IReadOnlyDictionary<string, string>? Data,
    DateTime? ExpiresAtUtc = null);
