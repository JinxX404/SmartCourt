namespace SmartCourt.Features.Notifications.DTOs;

public sealed record NotificationDto(
    Guid Id,
    string Type,
    string Severity,
    string Title,
    string Body,
    string? ActionUrl,
    IReadOnlyDictionary<string, string>? Data,
    DateTime CreatedAtUtc,
    DateTime? ReadAtUtc,
    DateTime? ExpiresAtUtc);
