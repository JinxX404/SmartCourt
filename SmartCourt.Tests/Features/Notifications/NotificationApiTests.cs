using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SmartCourt.Common.Models;
using SmartCourt.Features.Notifications.DTOs;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Persistence;
using SmartCourt.Tests.Common;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class NotificationApiTests(
    SmartCourtWebApplicationFactory factory)
    : IClassFixture<SmartCourtWebApplicationFactory>
{
    [Fact]
    public async Task Endpoints_RequireAuthentication()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/notifications");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task FeedAndMutations_AreOwnerScopedAndIdempotent()
    {
        var ownerId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        await factory.SeedUserAsync(ownerId, $"{ownerId:N}@test.local", "Client");
        await factory.SeedUserAsync(otherId, $"{otherId:N}@test.local", "Client");
        var ownerUnreadId = await SeedNotificationAsync(ownerId, 3, false);
        await SeedNotificationAsync(ownerId, 2, true);
        var otherIdNotification = await SeedNotificationAsync(otherId, 1, false);
        using var client = factory.CreateAuthenticatedClient(ownerId, "Client");

        var feed = await client.GetFromJsonAsync<ApiResponse<NotificationPageDto>>(
            "/api/notifications?pageSize=1&isRead=false");

        Assert.NotNull(feed?.Data);
        Assert.Single(feed.Data.Items);
        Assert.Equal(ownerUnreadId, feed.Data.Items[0].Id);
        Assert.Equal(1, feed.Data.UnreadCount);
        Assert.Null(feed.Data.NextCursor);
        Assert.DoesNotContain(feed.Data.Items, item => item.Id == otherIdNotification);

        var forbiddenAsNotFound = await client.PatchAsync(
            $"/api/notifications/{otherIdNotification}/read",
            null);
        Assert.Equal(HttpStatusCode.NotFound, forbiddenAsNotFound.StatusCode);

        var firstRead = await client.PatchAsync(
            $"/api/notifications/{ownerUnreadId}/read",
            null);
        var firstBody = await firstRead.Content
            .ReadFromJsonAsync<ApiResponse<NotificationDto>>();
        Assert.Equal(HttpStatusCode.OK, firstRead.StatusCode);
        Assert.NotNull(firstBody?.Data?.ReadAtUtc);

        var repeatedRead = await client.PatchAsync(
            $"/api/notifications/{ownerUnreadId}/read",
            null);
        var repeatedBody = await repeatedRead.Content
            .ReadFromJsonAsync<ApiResponse<NotificationDto>>();
        Assert.Equal(firstBody.Data.ReadAtUtc, repeatedBody?.Data?.ReadAtUtc);

        var count = await client.GetFromJsonAsync<
            ApiResponse<UnreadNotificationCountDto>>(
            "/api/notifications/unread-count");
        Assert.Equal(0, count?.Data?.UnreadCount);

        var readAll = await client.PatchAsync(
            "/api/notifications/read-all",
            null);
        var readAllBody = await readAll.Content
            .ReadFromJsonAsync<ApiResponse<NotificationsReadAllDto>>();
        Assert.Equal(HttpStatusCode.OK, readAll.StatusCode);
        Assert.Equal(0, readAllBody?.Data?.UnreadCount);
    }

    [Theory]
    [InlineData("?pageSize=0")]
    [InlineData("?pageSize=51")]
    [InlineData("?cursor=not-a-supported-cursor")]
    public async Task Feed_InvalidQuery_ReturnsBadRequest(string query)
    {
        var userId = Guid.NewGuid();
        await factory.SeedUserAsync(userId, $"{userId:N}@test.local", "Client");
        using var client = factory.CreateAuthenticatedClient(userId, "Client");

        var response = await client.GetAsync($"/api/notifications{query}");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<Guid> SeedNotificationAsync(
        Guid recipientId,
        long sequence,
        bool isRead)
    {
        await using var scope = factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        var now = DateTime.UtcNow.AddMinutes(-sequence);
        var notification = Notification.Create(
            Guid.NewGuid(),
            recipientId,
            Guid.NewGuid(),
            "proposal.created",
            NotificationSeverity.Information,
            "عرض جديد",
            "أرسل إليك موكل عرضًا جديدًا لمراجعته.",
            $"/proposals/{Guid.NewGuid()}",
            "{\"proposalId\":\"test\"}",
            now);
        notification.Sequence = DateTime.UtcNow.Ticks + sequence;
        if (isRead)
        {
            notification.MarkRead(now.AddSeconds(1));
        }

        dbContext.Notifications.Add(notification);
        await dbContext.SaveChangesAsync();
        return notification.Id;
    }
}
