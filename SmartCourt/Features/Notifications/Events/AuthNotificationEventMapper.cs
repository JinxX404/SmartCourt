using System.Text.Json;
using SmartCourt.Features.Auth.Integration;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class AuthNotificationEventMapper(
    IAuthNotificationContextReader contextReader)
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        AuthEventTypes.PasswordChanged,
        AuthEventTypes.PasswordReset
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (message.EventVersion != 1)
        {
            throw new InvalidOperationException(
                $"Authentication notification event version {message.EventVersion} is unsupported for '{message.EventType}'.");
        }

        if (message.EventType is not AuthEventTypes.PasswordChanged
            and not AuthEventTypes.PasswordReset)
        {
            throw new InvalidOperationException(
                $"Authentication notification event type '{message.EventType}' is unsupported.");
        }

        var payload = Deserialize(message);
        if (payload.UserId == Guid.Empty || payload.UserId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Authentication notification aggregate and payload identifiers do not match.");
        }

        var context = await contextReader.GetAccountAsync(
            payload.UserId,
            cancellationToken);
        if (context.UserId != payload.UserId)
        {
            throw new InvalidOperationException(
                "Authentication notification payload does not match its authoritative context.");
        }

        var contract = message.EventType == AuthEventTypes.PasswordChanged
            ? new SecurityNotificationContract(
                "security.password-changed",
                "تم تغيير كلمة المرور",
                "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم.")
            : new SecurityNotificationContract(
                "security.password-reset",
                "تمت إعادة تعيين كلمة المرور",
                "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم.");

        return
        [
            new NotificationDraft(
                context.UserId,
                contract.Type,
                NotificationSeverity.Critical,
                contract.Title,
                contract.Body,
                NotificationActionUrls.Security,
                new Dictionary<string, string>
                {
                    ["userId"] = context.UserId.ToString()
                })
        ];
    }

    private static AuthPasswordSecurityEventPayload Deserialize(
        OutboxMessage message)
    {
        try
        {
            return JsonSerializer.Deserialize<AuthPasswordSecurityEventPayload>(
                       message.Payload,
                       SerializerOptions)
                   ?? throw new JsonException();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Authentication notification payload is invalid.",
                exception);
        }
    }

    private sealed record SecurityNotificationContract(
        string Type,
        string Title,
        string Body);
}
