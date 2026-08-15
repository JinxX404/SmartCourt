using System.Text.Json;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Notifications.Enums;

namespace SmartCourt.Features.Notifications.Entities;

public sealed class Notification
{
    public const int MaximumTypeLength = 100;
    public const int MaximumTitleLength = 200;
    public const int MaximumBodyLength = 1_000;
    public const int MaximumActionUrlLength = 500;
    public const int MaximumDataJsonLength = 4_000;

    private Notification()
    {
    }

    private Notification(
        Guid id,
        Guid recipientUserId,
        Guid sourceEventId,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        string? actionUrl,
        string? dataJson,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        RecipientUserId = EntityGuard.NotEmpty(
            recipientUserId,
            nameof(recipientUserId));
        SourceEventId = EntityGuard.NotEmpty(
            sourceEventId,
            nameof(sourceEventId));
        Type = RequiredWithin(type, MaximumTypeLength, nameof(type));
        Severity = EnsureSeverity(severity);
        Title = RequiredWithin(title, MaximumTitleLength, nameof(title));
        Body = RequiredWithin(body, MaximumBodyLength, nameof(body));
        ActionUrl = ValidateActionUrl(actionUrl);
        DataJson = ValidateDataJson(dataJson);
        CreatedAtUtc = EntityGuard.Utc(createdAtUtc, nameof(createdAtUtc));
        ExpiresAtUtc = EntityGuard.OptionalUtc(
            expiresAtUtc,
            nameof(expiresAtUtc));
        if (ExpiresAtUtc.HasValue && ExpiresAtUtc <= CreatedAtUtc)
        {
            throw new BusinessException(
                "Notification expiry must be later than its creation time.");
        }
    }

    public Guid Id { get; internal set; }
    public long Sequence { get; internal set; }
    public Guid RecipientUserId { get; internal set; }
    public Guid SourceEventId { get; internal set; }
    public string Type { get; internal set; } = string.Empty;
    public NotificationSeverity Severity { get; internal set; }
    public string Title { get; internal set; } = string.Empty;
    public string Body { get; internal set; } = string.Empty;
    public string? ActionUrl { get; internal set; }
    public string? DataJson { get; internal set; }
    public DateTimeOffset CreatedAtUtc { get; internal set; }
    public DateTimeOffset? ReadAtUtc { get; internal set; }
    public DateTimeOffset? ExpiresAtUtc { get; internal set; }
    public byte[] RowVersion { get; internal set; } = [];
    public bool IsRead => ReadAtUtc.HasValue;

    public static Notification Create(
        Guid id,
        Guid recipientUserId,
        Guid sourceEventId,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        string? actionUrl,
        string? dataJson,
        DateTimeOffset createdAtUtc,
        DateTimeOffset? expiresAtUtc = null)
    {
        return new Notification(
            id,
            recipientUserId,
            sourceEventId,
            type,
            severity,
            title,
            body,
            actionUrl,
            dataJson,
            createdAtUtc,
            expiresAtUtc);
    }

    public bool MarkRead(DateTimeOffset readAtUtc)
    {
        if (ReadAtUtc.HasValue)
        {
            return false;
        }

        ReadAtUtc = EntityGuard.Utc(readAtUtc, nameof(readAtUtc));
        if (ReadAtUtc < CreatedAtUtc)
        {
            throw new BusinessException(
                "Notification read time cannot precede its creation time.");
        }

        return true;
    }

    private static string RequiredWithin(
        string value,
        int maximumLength,
        string fieldName)
    {
        var normalized = EntityGuard.Required(value, fieldName).Trim();
        if (normalized.Length > maximumLength)
        {
            throw new BusinessException(
                $"{fieldName} cannot exceed {maximumLength} characters.");
        }

        return normalized;
    }

    private static NotificationSeverity EnsureSeverity(
        NotificationSeverity severity)
    {
        if (!Enum.IsDefined(severity))
        {
            throw new BusinessException("Notification severity is invalid.");
        }

        return severity;
    }

    private static string? ValidateActionUrl(string? actionUrl)
    {
        if (string.IsNullOrWhiteSpace(actionUrl))
        {
            return null;
        }

        var normalized = actionUrl.Trim();
        if (normalized.Length > MaximumActionUrlLength
            || !normalized.StartsWith("/", StringComparison.Ordinal)
            || normalized.StartsWith("//", StringComparison.Ordinal)
            || normalized.Contains('\\')
            || normalized.Any(char.IsControl)
            || !Uri.TryCreate(normalized, UriKind.Relative, out _))
        {
            throw new BusinessException(
                "Notification action URL must be a safe relative application route.");
        }

        return normalized;
    }

    private static string? ValidateDataJson(string? dataJson)
    {
        if (string.IsNullOrWhiteSpace(dataJson))
        {
            return null;
        }

        var normalized = dataJson.Trim();
        if (normalized.Length > MaximumDataJsonLength)
        {
            throw new BusinessException(
                $"Notification data cannot exceed {MaximumDataJsonLength} characters.");
        }

        try
        {
            using var document = JsonDocument.Parse(normalized);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new BusinessException(
                    "Notification data must be a JSON object.");
            }
        }
        catch (JsonException)
        {
            throw new BusinessException("Notification data JSON is invalid.");
        }

        return normalized;
    }
}
