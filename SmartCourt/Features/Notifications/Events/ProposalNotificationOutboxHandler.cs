using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Features.Notifications.Entities;
using SmartCourt.Features.Notifications.Realtime;
using SmartCourt.Features.Notifications.Shared;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Notifications.Events;

public sealed class ProposalNotificationOutboxHandler(
    ApplicationDbContext dbContext,
    INotificationRealtimeNotifier realtimeNotifier) : IOutboxEventHandler
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ProposalCreated,
        ContractPaymentEventTypes.ProposalAccepted,
        ContractPaymentEventTypes.ProposalRejected
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        if (message.EventVersion != 1)
        {
            throw new InvalidOperationException(
                $"Proposal notification event version {message.EventVersion} is unsupported.");
        }

        var payload = JsonSerializer.Deserialize<ProposalEventPayload>(
            message.Payload,
            SerializerOptions)
            ?? throw new InvalidOperationException(
                "Proposal notification payload is invalid.");
        if (payload.ProposalId == Guid.Empty
            || payload.ProposalId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Proposal notification aggregate and payload identifiers do not match.");
        }

        var definition = ProposalNotificationMapper.Map(
            message.EventType,
            payload);
        var notification = await dbContext.Notifications
            .SingleOrDefaultAsync(item =>
                item.SourceEventId == message.Id
                && item.RecipientUserId == definition.RecipientUserId
                && item.Type == definition.Type,
                cancellationToken);

        if (notification is null)
        {
            var createdAtUtc = DateTime.SpecifyKind(
                message.CreatedAt,
                DateTimeKind.Utc);
            notification = Notification.Create(
                Guid.NewGuid(),
                definition.RecipientUserId,
                message.Id,
                definition.Type,
                definition.Severity,
                definition.Title,
                definition.Body,
                definition.ActionUrl,
                NotificationJson.Serialize(definition.Data),
                createdAtUtc);
            dbContext.Notifications.Add(notification);
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        await realtimeNotifier.NotificationCreatedAsync(
            notification.RecipientUserId,
            NotificationMapper.ToDto(notification),
            cancellationToken);
    }
}
