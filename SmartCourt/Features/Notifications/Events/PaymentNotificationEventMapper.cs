using System.Text.Json;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Features.Notifications.Enums;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;

namespace SmartCourt.Features.Notifications.Events;

internal sealed class PaymentNotificationEventMapper(
    IMilestoneNotificationContextReader milestoneContextReader,
    IPaymentNotificationContextReader paymentContextReader)
    : INotificationEventMapper
{
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web)
    {
        PropertyNameCaseInsensitive = true
    };

    public IReadOnlyCollection<string> EventTypes =>
    [
        ContractPaymentEventTypes.MilestoneFundingStarted,
        ContractPaymentEventTypes.MilestoneFunded,
        ContractPaymentEventTypes.MilestoneFundingFailed,
        ContractPaymentEventTypes.FundsReleased,
        ContractPaymentEventTypes.FundsRefunded,
        ContractPaymentEventTypes.WithdrawalCompleted,
        ContractPaymentEventTypes.WithdrawalFailed,
        ContractPaymentEventTypes.WithdrawalDelayed,
        ContractPaymentEventTypes.WalletAdjusted
    ];

    public async Task<IReadOnlyCollection<NotificationDraft>> MapAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        EnsureVersion(message, 1);
        if (IsFundingEvent(message.EventType))
        {
            return await MapFundingAsync(message, cancellationToken);
        }

        if (IsSettlementEvent(message.EventType))
        {
            return await MapSettlementAsync(message, cancellationToken);
        }

        if (IsWithdrawalEvent(message.EventType))
        {
            return await MapWithdrawalAsync(message, cancellationToken);
        }

        if (message.EventType == ContractPaymentEventTypes.WalletAdjusted)
        {
            return await MapAdjustmentAsync(message, cancellationToken);
        }

        throw Unsupported(message.EventType);
    }

    private async Task<IReadOnlyCollection<NotificationDraft>> MapFundingAsync(
        OutboxMessage message,
        CancellationToken cancellationToken)
    {
        var payload = Deserialize<ContractPaymentAggregateEventPayload>(message);
        EnsureIdentifier(payload.EntityId, "milestone");
        EnsureAggregate(message, payload.EntityId);
        var context = await milestoneContextReader.GetMilestoneAsync(
            payload.EntityId,
            cancellationToken);
        var data = MilestoneData(context);

        return message.EventType switch
        {
            ContractPaymentEventTypes.MilestoneFundingStarted =>
            [
                Draft(
                    context.LawyerUserId,
                    "milestone.funding-started",
                    NotificationSeverity.Information,
                    "بدأ تمويل المرحلة",
                    "بدأت معالجة تمويل المرحلة. انتظر تأكيد اكتمال التمويل قبل بدء العمل.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneFunded =>
            [
                Draft(
                    context.ClientUserId,
                    "milestone.funded",
                    NotificationSeverity.Success,
                    "تم تمويل المرحلة",
                    "اكتمل تمويل المرحلة وحُفظ المبلغ في حساب الضمان.",
                    data),
                Draft(
                    context.LawyerUserId,
                    "milestone.funded",
                    NotificationSeverity.Success,
                    "تم تمويل المرحلة",
                    context.Type
                        == SmartCourt.Features.Milestones.Enums.MilestoneType.Expense
                        ? "اكتمل تمويل المصروف وبدأ تحريره مباشرة إلى حسابك."
                        : "اكتمل تمويل المرحلة، ويمكنك الآن بدء العمل عليها.",
                    data)
            ],
            ContractPaymentEventTypes.MilestoneFundingFailed =>
            [
                Draft(
                    context.ClientUserId,
                    "milestone.funding-failed",
                    NotificationSeverity.Critical,
                    "فشل تمويل المرحلة",
                    "لم تكتمل عملية تمويل المرحلة. يمكنك مراجعة وسيلة الدفع والمحاولة مرة أخرى.",
                    data)
            ],
            _ => throw Unsupported(message.EventType)
        };
    }

    private async Task<IReadOnlyCollection<NotificationDraft>>
        MapSettlementAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
    {
        var payload = ReadSettlement(message);
        var context = await milestoneContextReader.GetMilestoneAsync(
            payload.MilestoneId,
            cancellationToken);
        var data = SettlementData(
            context,
            payload.EscrowHoldId,
            payload.PaymentTransactionId);

        return message.EventType switch
        {
            ContractPaymentEventTypes.FundsReleased =>
            [
                Draft(
                    context.ClientUserId,
                    "funds.released",
                    NotificationSeverity.Success,
                    "تم تحرير أموال المرحلة",
                    "انتهت مدة الحجز وتم تحرير مستحقات المحامي عن المرحلة.",
                    data),
                Draft(
                    context.LawyerUserId,
                    "funds.released",
                    NotificationSeverity.Success,
                    "أصبحت مستحقات المرحلة متاحة",
                    "تم تحويل مستحقات المرحلة إلى رصيد محفظتك المتاح.",
                    data)
            ],
            ContractPaymentEventTypes.FundsRefunded =>
            [
                Draft(
                    context.ClientUserId,
                    "funds.refunded",
                    NotificationSeverity.Success,
                    "تم رد أموال المرحلة",
                    "اكتملت تسوية المرحلة وتم رد الأموال إلى العميل.",
                    data),
                Draft(
                    context.LawyerUserId,
                    "funds.refunded",
                    NotificationSeverity.Information,
                    "تم رد أموال المرحلة",
                    "اكتملت تسوية المرحلة برد الأموال إلى العميل.",
                    data)
            ],
            _ => throw Unsupported(message.EventType)
        };
    }

    private async Task<IReadOnlyCollection<NotificationDraft>>
        MapWithdrawalAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
    {
        var payload = Deserialize<WithdrawalOutcomeEventPayload>(message);
        EnsureIdentifier(payload.WithdrawalId, "withdrawal");
        EnsureIdentifier(payload.LawyerUserId, "lawyer");
        EnsureAggregate(message, payload.WithdrawalId);
        var context = await paymentContextReader.GetWithdrawalAsync(
            payload.WithdrawalId,
            cancellationToken);
        if (context.LawyerUserId != payload.LawyerUserId)
        {
            throw new InvalidOperationException(
                "Withdrawal notification recipient does not match its payment context.");
        }

        EnsureWithdrawalState(message.EventType, context);
        var data = new Dictionary<string, string>
        {
            ["withdrawalId"] = context.WithdrawalId.ToString()
        };
        return message.EventType switch
        {
            ContractPaymentEventTypes.WithdrawalCompleted =>
            [
                Draft(
                    context.LawyerUserId,
                    "wallet.withdrawal-completed",
                    NotificationSeverity.Success,
                    "اكتمل طلب السحب",
                    "اكتمل طلب سحب الرصيد من محفظتك بنجاح.",
                    data)
            ],
            ContractPaymentEventTypes.WithdrawalFailed =>
            [
                Draft(
                    context.LawyerUserId,
                    "wallet.withdrawal-failed",
                    NotificationSeverity.Warning,
                    "فشل طلب السحب",
                    "لم يكتمل طلب السحب، وأُعيد المبلغ إلى رصيد محفظتك المتاح.",
                    data)
            ],
            ContractPaymentEventTypes.WithdrawalDelayed =>
            [
                Draft(
                    context.LawyerUserId,
                    "wallet.withdrawal-delayed",
                    NotificationSeverity.Warning,
                    "طلب السحب يحتاج إلى مراجعة",
                    "تأخر حسم طلب السحب ويجري التعامل معه يدويًا. لا تنشئ طلبًا بديلًا.",
                    data)
            ],
            _ => throw Unsupported(message.EventType)
        };
    }

    private async Task<IReadOnlyCollection<NotificationDraft>>
        MapAdjustmentAsync(
            OutboxMessage message,
            CancellationToken cancellationToken)
    {
        var payload = Deserialize<WalletAdjustedEventPayload>(message);
        EnsureIdentifier(payload.WalletAdjustmentId, "wallet adjustment");
        EnsureIdentifier(payload.LawyerUserId, "lawyer");
        EnsureIdentifier(payload.ContractId, "contract");
        EnsureAggregate(message, payload.WalletAdjustmentId);
        var context = await paymentContextReader.GetWalletAdjustmentAsync(
            payload.WalletAdjustmentId,
            cancellationToken);
        if (context.LawyerUserId != payload.LawyerUserId
            || context.ContractId != payload.ContractId)
        {
            throw new InvalidOperationException(
                "Wallet adjustment notification payload does not match its payment context.");
        }

        return
        [
            Draft(
                context.LawyerUserId,
                "wallet.adjusted",
                NotificationSeverity.Warning,
                "تم تصحيح رصيد المحفظة",
                "أجرى مسؤول النظام تصحيحًا ماليًا على محفظتك. راجع الرصيد الحالي والتفاصيل مع الدعم عند الحاجة.",
                new Dictionary<string, string>
                {
                    ["walletAdjustmentId"] =
                        context.WalletAdjustmentId.ToString(),
                    ["contractId"] = context.ContractId.ToString()
                })
        ];
    }

    private static SettlementEventPayload ReadSettlement(OutboxMessage message)
    {
        if (message.EventType == ContractPaymentEventTypes.FundsReleased)
        {
            var payload = Deserialize<FundsReleasedEventPayload>(message);
            EnsureSettlementIdentifiers(
                message,
                payload.MilestoneId,
                payload.EscrowHoldId,
                payload.PaymentTransactionId);
            if (payload.LawyerNetAmount <= 0m
                || payload.PlatformFeeAmount < 0m)
            {
                throw new InvalidOperationException(
                    "Released-funds notification amounts are invalid.");
            }

            return new SettlementEventPayload(
                payload.MilestoneId,
                payload.EscrowHoldId,
                payload.PaymentTransactionId);
        }

        var refunded = Deserialize<FundsRefundedEventPayload>(message);
        EnsureSettlementIdentifiers(
            message,
            refunded.MilestoneId,
            refunded.EscrowHoldId,
            refunded.PaymentTransactionId);
        if (refunded.ClientRefundAmount <= 0m)
        {
            throw new InvalidOperationException(
                "Refunded-funds notification amount is invalid.");
        }

        return new SettlementEventPayload(
            refunded.MilestoneId,
            refunded.EscrowHoldId,
            refunded.PaymentTransactionId);
    }

    private static void EnsureSettlementIdentifiers(
        OutboxMessage message,
        Guid milestoneId,
        Guid escrowHoldId,
        Guid paymentTransactionId)
    {
        EnsureIdentifier(milestoneId, "milestone");
        EnsureIdentifier(escrowHoldId, "escrow hold");
        EnsureIdentifier(paymentTransactionId, "payment transaction");
        EnsureAggregate(message, escrowHoldId);
    }

    private static void EnsureWithdrawalState(
        string eventType,
        WithdrawalNotificationContext context)
    {
        var valid = eventType switch
        {
            ContractPaymentEventTypes.WithdrawalCompleted =>
                context.Status == WithdrawalStatus.Completed
                && !context.RequiresManualAction,
            ContractPaymentEventTypes.WithdrawalFailed =>
                context.Status == WithdrawalStatus.Failed
                && !context.RequiresManualAction,
            ContractPaymentEventTypes.WithdrawalDelayed =>
                context.Status == WithdrawalStatus.Processing
                && context.RequiresManualAction,
            _ => false
        };
        if (!valid)
        {
            throw new InvalidOperationException(
                "Withdrawal notification event does not match its current payment state.");
        }
    }

    private static IReadOnlyDictionary<string, string> MilestoneData(
        MilestoneNotificationContext context) =>
        new Dictionary<string, string>
        {
            ["milestoneId"] = context.MilestoneId.ToString(),
            ["contractId"] = context.ContractId.ToString(),
            ["proposalId"] = context.ProposalId.ToString(),
            ["legalCaseId"] = context.LegalCaseId.ToString()
        };

    private static IReadOnlyDictionary<string, string> SettlementData(
        MilestoneNotificationContext context,
        Guid escrowHoldId,
        Guid paymentTransactionId)
    {
        var data = new Dictionary<string, string>(MilestoneData(context))
        {
            ["escrowHoldId"] = escrowHoldId.ToString(),
            ["paymentTransactionId"] = paymentTransactionId.ToString()
        };
        return data;
    }

    private static NotificationDraft Draft(
        Guid recipientUserId,
        string type,
        NotificationSeverity severity,
        string title,
        string body,
        IReadOnlyDictionary<string, string> data) => new(
            recipientUserId,
            type,
            severity,
            title,
            body,
            null,
            data);

    private static bool IsFundingEvent(string eventType) =>
        eventType is ContractPaymentEventTypes.MilestoneFundingStarted
            or ContractPaymentEventTypes.MilestoneFunded
            or ContractPaymentEventTypes.MilestoneFundingFailed;

    private static bool IsSettlementEvent(string eventType) =>
        eventType is ContractPaymentEventTypes.FundsReleased
            or ContractPaymentEventTypes.FundsRefunded;

    private static bool IsWithdrawalEvent(string eventType) =>
        eventType is ContractPaymentEventTypes.WithdrawalCompleted
            or ContractPaymentEventTypes.WithdrawalFailed
            or ContractPaymentEventTypes.WithdrawalDelayed;

    private static T Deserialize<T>(OutboxMessage message)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(
                    message.Payload,
                    SerializerOptions)
                ?? throw new JsonException();
        }
        catch (JsonException exception)
        {
            throw new InvalidOperationException(
                "Payment notification payload is invalid.",
                exception);
        }
    }

    private static void EnsureVersion(
        OutboxMessage message,
        int expectedVersion)
    {
        if (message.EventVersion != expectedVersion)
        {
            throw new InvalidOperationException(
                $"Payment notification event version {message.EventVersion} is unsupported for '{message.EventType}'.");
        }
    }

    private static void EnsureAggregate(
        OutboxMessage message,
        Guid aggregateId)
    {
        if (aggregateId == Guid.Empty || aggregateId != message.AggregateId)
        {
            throw new InvalidOperationException(
                "Payment notification aggregate and payload identifiers do not match.");
        }
    }

    private static void EnsureIdentifier(Guid identifier, string name)
    {
        if (identifier == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Payment notification {name} identifier is invalid.");
        }
    }

    private static InvalidOperationException Unsupported(string eventType) =>
        new($"Payment notification event type '{eventType}' is unsupported.");

    private sealed record SettlementEventPayload(
        Guid MilestoneId,
        Guid EscrowHoldId,
        Guid PaymentTransactionId);
}
