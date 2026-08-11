using System.Text.Json;
using SmartCourt.Features.Auth.Integration;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class AuthNotificationEventMapperTests
{
    private static readonly Guid UserId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(
        2026,
        8,
        11,
        12,
        0,
        0,
        DateTimeKind.Utc);

    public static TheoryData<
        string,
        string,
        string,
        string> SecurityCases => new()
        {
            {
                AuthEventTypes.PasswordChanged,
                "security.password-changed",
                "تم تغيير كلمة المرور",
                "تم تغيير كلمة مرور حسابك بنجاح. إذا لم تكن أنت من أجرى هذا التغيير، يرجى تأمين حسابك والتواصل مع الدعم."
            },
            {
                AuthEventTypes.PasswordReset,
                "security.password-reset",
                "تمت إعادة تعيين كلمة المرور",
                "تمت إعادة تعيين كلمة مرور حسابك بنجاح. إذا لم تطلب هذا الإجراء، يرجى تأمين حسابك والتواصل مع الدعم."
            }
        };

    [Theory]
    [MemberData(nameof(SecurityCases))]
    public async Task MapAsync_UsesExactCriticalArabicCopyAndSafeAccountData(
        string eventType,
        string expectedType,
        string expectedTitle,
        string expectedBody)
    {
        var mapper = CreateMapper();

        var draft = Assert.Single(await mapper.MapAsync(
            CreateMessage(eventType, new AuthPasswordSecurityEventPayload(UserId)),
            CancellationToken.None));

        Assert.Equal(UserId, draft.RecipientUserId);
        Assert.Equal(expectedType, draft.Type);
        Assert.Equal("Critical", draft.Severity.ToString());
        Assert.Equal(expectedTitle, draft.Title);
        Assert.Equal(expectedBody, draft.Body);
        Assert.Null(draft.ActionUrl);
        Assert.Equal(UserId.ToString(), draft.Data!["userId"]);
        Assert.Single(draft.Data);
        Assert.DoesNotContain(
            draft.Data.Keys,
            key => key.Contains("email", StringComparison.OrdinalIgnoreCase)
                || key.Contains("token", StringComparison.OrdinalIgnoreCase)
                || key.Contains("ip", StringComparison.OrdinalIgnoreCase)
                || key.Contains("device", StringComparison.OrdinalIgnoreCase)
                || key.Contains("stamp", StringComparison.OrdinalIgnoreCase)
                || key.Contains("password", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void EventTypes_AdvertisesBothSecurityEventsExactlyOnce()
    {
        var mapper = CreateMapper();

        Assert.Equal(2, mapper.EventTypes.Count);
        Assert.Equal(2, mapper.EventTypes.Distinct().Count());
        Assert.Contains(AuthEventTypes.PasswordChanged, mapper.EventTypes);
        Assert.Contains(AuthEventTypes.PasswordReset, mapper.EventTypes);
    }

    [Fact]
    public async Task MapAsync_RejectsUnsupportedVersionAggregateAndContextMismatch()
    {
        var payload = new AuthPasswordSecurityEventPayload(UserId);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper().MapAsync(
                CreateMessage(AuthEventTypes.PasswordChanged, payload, eventVersion: 2),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper().MapAsync(
                CreateMessage(
                    AuthEventTypes.PasswordChanged,
                    payload,
                    aggregateId: Guid.NewGuid()),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper(Guid.NewGuid()).MapAsync(
                CreateMessage(AuthEventTypes.PasswordChanged, payload),
                CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_RejectsMalformedPayloadAndUnsupportedEvent()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper().MapAsync(
                CreateRawMessage(AuthEventTypes.PasswordReset, "{"),
                CancellationToken.None));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper().MapAsync(
                CreateMessage("UnrelatedEvent", new AuthPasswordSecurityEventPayload(UserId)),
                CancellationToken.None));
    }

    private static AuthNotificationEventMapper CreateMapper(Guid? contextUserId = null) =>
        new(new StubContextReader(
            new AuthAccountNotificationContext(contextUserId ?? UserId)));

    private static OutboxMessage CreateMessage(
        string eventType,
        AuthPasswordSecurityEventPayload payload,
        int eventVersion = 1,
        Guid? aggregateId = null) =>
        CreateRawMessage(
            eventType,
            JsonSerializer.Serialize(payload),
            eventVersion,
            aggregateId ?? UserId);

    private static OutboxMessage CreateRawMessage(
        string eventType,
        string payload,
        int eventVersion = 1,
        Guid? aggregateId = null) =>
        new(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            payload,
            nameof(ApplicationUser),
            aggregateId ?? UserId,
            Guid.NewGuid(),
            UtcNow,
            UtcNow);

    private sealed class StubContextReader(
        AuthAccountNotificationContext context) : IAuthNotificationContextReader
    {
        public Task<AuthAccountNotificationContext> GetAccountAsync(
            Guid userId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(UserId, userId);
            return Task.FromResult(context);
        }
    }
}
