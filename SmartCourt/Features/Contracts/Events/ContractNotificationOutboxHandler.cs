using SmartCourt.Features.Notifications.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Contracts.Events;

public sealed class ContractNotificationOutboxHandler
    : IOutboxEventHandler
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IReadOnlyCollection<IContractNotificationService>
        _notificationServices;

    public ContractNotificationOutboxHandler(
        ApplicationDbContext dbContext,
        IEnumerable<IContractNotificationService> notificationServices)
    {
        _dbContext = dbContext;
        _notificationServices = notificationServices.ToArray();
    }

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.ContractCreated,
        ContractPaymentEventTypes.ContractAccepted,
        ContractPaymentEventTypes.ContractActivated,
        ContractPaymentEventTypes.ContractTerminated,
        ContractPaymentEventTypes.MilestoneReadyForFunding,
        ContractPaymentEventTypes.MilestoneFundingStarted,
        ContractPaymentEventTypes.MilestoneFunded,
        ContractPaymentEventTypes.MilestoneFundingFailed,
        ContractPaymentEventTypes.MilestoneSubmitted,
        ContractPaymentEventTypes.MilestoneAutoAccepted,
        ContractPaymentEventTypes.MilestoneAccepted,
        ContractPaymentEventTypes.MilestoneChangesRequested,
        ContractPaymentEventTypes.MilestoneChangeRequestCreated,
        ContractPaymentEventTypes.FundsReleased,
        ContractPaymentEventTypes.FundsRefunded,
        ContractPaymentEventTypes.DisputeOpened,
        ContractPaymentEventTypes.DisputeAssigned,
        ContractPaymentEventTypes.DisputeResolved,
        ContractPaymentEventTypes.DisputeClosed
    ];

    public async Task HandleAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var notificationService = GetNotificationService();
        var context = await new ContractIntegrationEventResolver(_dbContext)
            .ResolveAsync(message, cancellationToken);
        var recipients = new[]
            {
                context.ClientUserId,
                context.LawyerUserId,
                context.ModeratorUserId
            }
            .Where(userId => userId.HasValue && userId != Guid.Empty)
            .Select(userId => userId!.Value)
            .Distinct()
            .ToArray();
        foreach (var recipientUserId in recipients)
        {
            await notificationService.PublishAsync(
                new ContractNotification(
                    message.Id,
                    recipientUserId,
                    MapType(message.EventType),
                    context.RelatedEntityType,
                    context.RelatedEntityId),
                cancellationToken);
        }
    }

    private IContractNotificationService GetNotificationService()
    {
        if (_notificationServices.Count == 1)
        {
            return _notificationServices.Single();
        }

        throw new InvalidOperationException(
            "خدمة إشعارات العقود غير متاحة، وسيعاد إرسال الحدث تلقائيًا.");
    }

    private static ContractNotificationType MapType(string eventType)
    {
        return eventType switch
        {
            ContractPaymentEventTypes.ContractCreated =>
                ContractNotificationType.ContractCreated,
            ContractPaymentEventTypes.ContractAccepted =>
                ContractNotificationType.ContractAccepted,
            ContractPaymentEventTypes.ContractActivated =>
                ContractNotificationType.ContractActivated,
            ContractPaymentEventTypes.ContractTerminated =>
                ContractNotificationType.ContractTerminated,
            ContractPaymentEventTypes.MilestoneReadyForFunding =>
                ContractNotificationType.MilestoneReadyForFunding,
            ContractPaymentEventTypes.MilestoneFundingStarted =>
                ContractNotificationType.MilestoneFundingStarted,
            ContractPaymentEventTypes.MilestoneFunded =>
                ContractNotificationType.MilestoneFunded,
            ContractPaymentEventTypes.MilestoneFundingFailed =>
                ContractNotificationType.MilestoneFundingFailed,
            ContractPaymentEventTypes.MilestoneSubmitted =>
                ContractNotificationType.MilestoneSubmitted,
            ContractPaymentEventTypes.MilestoneAutoAccepted =>
                ContractNotificationType.MilestoneAutoAccepted,
            ContractPaymentEventTypes.MilestoneAccepted =>
                ContractNotificationType.MilestoneAccepted,
            ContractPaymentEventTypes.MilestoneChangesRequested
                or ContractPaymentEventTypes.MilestoneChangeRequestCreated =>
                ContractNotificationType.MilestoneChangesRequested,
            ContractPaymentEventTypes.FundsReleased =>
                ContractNotificationType.FundsReleased,
            ContractPaymentEventTypes.FundsRefunded =>
                ContractNotificationType.FundsRefunded,
            ContractPaymentEventTypes.DisputeOpened =>
                ContractNotificationType.DisputeOpened,
            ContractPaymentEventTypes.DisputeAssigned =>
                ContractNotificationType.DisputeAssigned,
            ContractPaymentEventTypes.DisputeResolved =>
                ContractNotificationType.DisputeResolved,
            ContractPaymentEventTypes.DisputeClosed =>
                ContractNotificationType.DisputeClosed,
            _ => throw new InvalidOperationException(
                "نوع حدث إشعارات العقود غير مدعوم.")
        };
    }
}
