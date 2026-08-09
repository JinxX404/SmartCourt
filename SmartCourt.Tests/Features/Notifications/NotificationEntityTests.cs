using SmartCourt.Common.Exceptions;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Enums;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class NotificationEntityTests
{
    private static readonly DateTime CreatedAtUtc = new(
        2026,
        8,
        9,
        10,
        0,
        0,
        DateTimeKind.Utc);

    [Fact]
    public void Create_NormalizesSafeContent_AndMarkReadIsIdempotent()
    {
        var notification = Create(actionUrl: " /proposals/123 ");

        Assert.Equal("/proposals/123", notification.ActionUrl);
        Assert.False(notification.IsRead);
        Assert.True(notification.MarkRead(CreatedAtUtc.AddMinutes(1)));
        Assert.False(notification.MarkRead(CreatedAtUtc.AddMinutes(2)));
        Assert.Equal(CreatedAtUtc.AddMinutes(1), notification.ReadAtUtc);
    }

    [Theory]
    [InlineData("https://evil.example/path")]
    [InlineData("//evil.example/path")]
    [InlineData("/proposals\\123")]
    public void Create_RejectsUnsafeActionUrl(string actionUrl)
    {
        Assert.Throws<BusinessException>(() => Create(actionUrl));
    }

    [Theory]
    [InlineData("[]")]
    [InlineData("not-json")]
    public void Create_RejectsNonObjectOrInvalidDataJson(string dataJson)
    {
        Assert.Throws<BusinessException>(() => Create(dataJson: dataJson));
    }

    private static Notification Create(
        string? actionUrl = "/proposals/123",
        string? dataJson = "{\"proposalId\":\"123\"}")
    {
        return Notification.Create(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            "proposal.created",
            NotificationSeverity.Information,
            "New proposal",
            "A client sent you a new proposal.",
            actionUrl,
            dataJson,
            CreatedAtUtc);
    }
}
