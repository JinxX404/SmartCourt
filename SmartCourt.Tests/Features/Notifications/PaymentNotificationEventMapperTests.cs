using System.Text.Json;
using SmartCourt.Features.Milestones.Integration;
using SmartCourt.Features.Notifications.Events;
using SmartCourt.Features.Payments.Enums;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Infrastructure.Providers.Events;
using Xunit;

namespace SmartCourt.Tests.Features.Notifications;

public sealed class PaymentNotificationEventMapperTests
{
    private static readonly Guid MilestoneId = Guid.NewGuid();
    private static readonly Guid ContractId = Guid.NewGuid();
    private static readonly Guid ProposalId = Guid.NewGuid();
    private static readonly Guid LegalCaseId = Guid.NewGuid();
    private static readonly Guid ClientUserId = Guid.NewGuid();
    private static readonly Guid LawyerUserId = Guid.NewGuid();
    private static readonly Guid EscrowHoldId = Guid.NewGuid();
    private static readonly Guid PaymentTransactionId = Guid.NewGuid();
    private static readonly Guid WithdrawalId = Guid.NewGuid();
    private static readonly Guid AdjustmentId = Guid.NewGuid();
    private static readonly DateTime UtcNow = new(
        2026,
        8,
        9,
        12,
        0,
        0,
        DateTimeKind.Utc);

    [Fact]
    public void EventTypes_AdvertisesEveryGateThreeContractExactlyOnce()
    {
        var mapper = CreateMapper();

        Assert.Equal(9, mapper.EventTypes.Count);
        Assert.Equal(9, mapper.EventTypes.Distinct().Count());
        Assert.Contains(
            ContractPaymentEventTypes.MilestoneFundingStarted,
            mapper.EventTypes);
        Assert.Contains(
            ContractPaymentEventTypes.FundsRefunded,
            mapper.EventTypes);
        Assert.Contains(
            ContractPaymentEventTypes.WithdrawalDelayed,
            mapper.EventTypes);
        Assert.Contains(
            ContractPaymentEventTypes.WalletAdjusted,
            mapper.EventTypes);
    }

    [Fact]
    public async Task MapAsync_FundingStartedNotifiesOnlyLawyerWithExactArabic()
    {
        var drafts = await CreateMapper().MapAsync(
            FundingMessage(ContractPaymentEventTypes.MilestoneFundingStarted),
            CancellationToken.None);

        var draft = Assert.Single(drafts);
        AssertDraft(
            draft,
            LawyerUserId,
            "milestone.funding-started",
            "Information",
            "بدأ تمويل المرحلة",
            "بدأت معالجة تمويل المرحلة. انتظر تأكيد اكتمال التمويل قبل بدء العمل.");
        AssertMilestoneData(draft.Data!);
    }

    [Fact]
    public async Task MapAsync_FundedUsesRoleSpecificArabicForBothParticipants()
    {
        var drafts = await CreateMapper().MapAsync(
            FundingMessage(ContractPaymentEventTypes.MilestoneFunded),
            CancellationToken.None);

        Assert.Collection(
            drafts.OrderBy(item => item.RecipientUserId),
            first => AssertFundedVariant(first),
            second => AssertFundedVariant(second));
        Assert.Contains(drafts, item =>
            item.RecipientUserId == ClientUserId
            && item.Body
                == "اكتمل تمويل المرحلة وحُفظ المبلغ في حساب الضمان.");
        Assert.Contains(drafts, item =>
            item.RecipientUserId == LawyerUserId
            && item.Body
                == "اكتمل تمويل المرحلة، ويمكنك الآن بدء العمل عليها.");
    }

    [Fact]
    public async Task MapAsync_FundingFailedNotifiesOnlyClient()
    {
        var draft = Assert.Single(await CreateMapper().MapAsync(
            FundingMessage(ContractPaymentEventTypes.MilestoneFundingFailed),
            CancellationToken.None));

        AssertDraft(
            draft,
            ClientUserId,
            "milestone.funding-failed",
            "Critical",
            "فشل تمويل المرحلة",
            "لم تكتمل عملية تمويل المرحلة. يمكنك مراجعة وسيلة الدفع والمحاولة مرة أخرى.");
        AssertMilestoneData(draft.Data!);
    }

    [Fact]
    public async Task MapAsync_FundsReleasedUsesRoleSpecificArabicAndSafeData()
    {
        var message = CreateMessage(
            ContractPaymentEventTypes.FundsReleased,
            EscrowHoldId,
            new FundsReleasedEventPayload(
                MilestoneId,
                EscrowHoldId,
                PaymentTransactionId,
                900m,
                100m),
            "EscrowHold");

        var drafts = await CreateMapper().MapAsync(
            message,
            CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Contains(drafts, item =>
            item.RecipientUserId == ClientUserId
            && item.Type == "funds.released"
            && item.Title == "تم تحرير أموال المرحلة"
            && item.Body
                == "انتهت مدة الحجز وتم تحرير مستحقات المحامي عن المرحلة.");
        Assert.Contains(drafts, item =>
            item.RecipientUserId == LawyerUserId
            && item.Type == "funds.released"
            && item.Title == "أصبحت مستحقات المرحلة متاحة"
            && item.Body
                == "تم تحويل مستحقات المرحلة إلى رصيد محفظتك المتاح.");
        Assert.All(drafts, draft =>
        {
            Assert.Equal("Success", draft.Severity.ToString());
            AssertSettlementData(draft.Data!);
        });
    }

    [Fact]
    public async Task MapAsync_FundsRefundedUsesRoleSpecificSeverityAndSafeData()
    {
        var message = CreateMessage(
            ContractPaymentEventTypes.FundsRefunded,
            EscrowHoldId,
            new FundsRefundedEventPayload(
                MilestoneId,
                EscrowHoldId,
                PaymentTransactionId,
                1_000m),
            "EscrowHold");

        var drafts = await CreateMapper().MapAsync(
            message,
            CancellationToken.None);

        Assert.Equal(2, drafts.Count);
        Assert.Contains(drafts, item =>
            item.RecipientUserId == ClientUserId
            && item.Severity.ToString() == "Success"
            && item.Body
                == "اكتملت تسوية المرحلة وتم رد الأموال إلى العميل.");
        Assert.Contains(drafts, item =>
            item.RecipientUserId == LawyerUserId
            && item.Severity.ToString() == "Information"
            && item.Body
                == "اكتملت تسوية المرحلة برد الأموال إلى العميل.");
        Assert.All(drafts, draft =>
        {
            Assert.Equal("funds.refunded", draft.Type);
            Assert.Equal("تم رد أموال المرحلة", draft.Title);
            AssertSettlementData(draft.Data!);
        });
    }

    public static TheoryData<
        string,
        WithdrawalStatus,
        bool,
        string,
        string,
        string,
        string> WithdrawalCases => new()
        {
            {
                ContractPaymentEventTypes.WithdrawalCompleted,
                WithdrawalStatus.Completed,
                false,
                "wallet.withdrawal-completed",
                "Success",
                "اكتمل طلب السحب",
                "اكتمل طلب سحب الرصيد من محفظتك بنجاح."
            },
            {
                ContractPaymentEventTypes.WithdrawalFailed,
                WithdrawalStatus.Failed,
                false,
                "wallet.withdrawal-failed",
                "Warning",
                "فشل طلب السحب",
                "لم يكتمل طلب السحب، وأُعيد المبلغ إلى رصيد محفظتك المتاح."
            },
            {
                ContractPaymentEventTypes.WithdrawalDelayed,
                WithdrawalStatus.Processing,
                true,
                "wallet.withdrawal-delayed",
                "Warning",
                "طلب السحب يحتاج إلى مراجعة",
                "تأخر حسم طلب السحب ويجري التعامل معه يدويًا. لا تنشئ طلبًا بديلًا."
            }
        };

    [Theory]
    [MemberData(nameof(WithdrawalCases))]
    public async Task MapAsync_WithdrawalOutcomeUsesExactContract(
        string eventType,
        WithdrawalStatus status,
        bool requiresManualAction,
        string expectedType,
        string expectedSeverity,
        string expectedTitle,
        string expectedBody)
    {
        var mapper = CreateMapper(new WithdrawalNotificationContext(
            WithdrawalId,
            LawyerUserId,
            status,
            requiresManualAction));
        var message = CreateMessage(
            eventType,
            WithdrawalId,
            new WithdrawalOutcomeEventPayload(
                WithdrawalId,
                LawyerUserId),
            "WithdrawalRequest");

        var draft = Assert.Single(await mapper.MapAsync(
            message,
            CancellationToken.None));

        AssertDraft(
            draft,
            LawyerUserId,
            expectedType,
            expectedSeverity,
            expectedTitle,
            expectedBody);
        Assert.Equal(WithdrawalId.ToString(), draft.Data!["withdrawalId"]);
        Assert.Single(draft.Data);
    }

    [Fact]
    public async Task MapAsync_WalletAdjustmentUsesSafeContract()
    {
        var message = CreateMessage(
            ContractPaymentEventTypes.WalletAdjusted,
            AdjustmentId,
            new WalletAdjustedEventPayload(
                AdjustmentId,
                LawyerUserId,
                ContractId),
            "WalletAdjustment");

        var draft = Assert.Single(await CreateMapper().MapAsync(
            message,
            CancellationToken.None));

        AssertDraft(
            draft,
            LawyerUserId,
            "wallet.adjusted",
            "Warning",
            "تم تصحيح رصيد المحفظة",
            "أجرى مسؤول النظام تصحيحًا ماليًا على محفظتك. راجع الرصيد الحالي والتفاصيل مع الدعم عند الحاجة.");
        Assert.Equal(AdjustmentId.ToString(), draft.Data!["walletAdjustmentId"]);
        Assert.Equal(ContractId.ToString(), draft.Data["contractId"]);
        Assert.Equal(2, draft.Data.Count);
    }

    [Fact]
    public async Task MapAsync_RejectsUnsupportedVersionAndAggregateMismatch()
    {
        var mapper = CreateMapper();
        var unsupportedVersion = FundingMessage(
            ContractPaymentEventTypes.MilestoneFunded,
            eventVersion: 2);
        var mismatchedAggregate = CreateMessage(
            ContractPaymentEventTypes.MilestoneFunded,
            Guid.NewGuid(),
            new ContractPaymentAggregateEventPayload(MilestoneId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                unsupportedVersion,
                CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            mapper.MapAsync(
                mismatchedAggregate,
                CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_RejectsWithdrawalRecipientOrStateMismatch()
    {
        var stateMismatch = CreateMapper(new WithdrawalNotificationContext(
            WithdrawalId,
            LawyerUserId,
            WithdrawalStatus.Processing,
            false));
        var recipientMismatch = CreateMapper(new WithdrawalNotificationContext(
            WithdrawalId,
            Guid.NewGuid(),
            WithdrawalStatus.Completed,
            false));
        var message = CreateMessage(
            ContractPaymentEventTypes.WithdrawalCompleted,
            WithdrawalId,
            new WithdrawalOutcomeEventPayload(
                WithdrawalId,
                LawyerUserId));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            stateMismatch.MapAsync(message, CancellationToken.None));
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            recipientMismatch.MapAsync(message, CancellationToken.None));
    }

    [Fact]
    public async Task MapAsync_RejectsInvalidSettlementAmounts()
    {
        var message = CreateMessage(
            ContractPaymentEventTypes.FundsReleased,
            EscrowHoldId,
            new FundsReleasedEventPayload(
                MilestoneId,
                EscrowHoldId,
                PaymentTransactionId,
                0m,
                0m));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            CreateMapper().MapAsync(message, CancellationToken.None));
    }

    private static PaymentNotificationEventMapper CreateMapper(
        WithdrawalNotificationContext? withdrawal = null) => new(
            new StubMilestoneContextReader(),
            new StubPaymentContextReader(
                withdrawal ?? new WithdrawalNotificationContext(
                    WithdrawalId,
                    LawyerUserId,
                    WithdrawalStatus.Completed,
                    false)));

    private static OutboxMessage FundingMessage(
        string eventType,
        int eventVersion = 1) => CreateMessage(
            eventType,
            MilestoneId,
            new ContractPaymentAggregateEventPayload(MilestoneId),
            eventVersion: eventVersion);

    private static OutboxMessage CreateMessage<T>(
        string eventType,
        Guid aggregateId,
        T payload,
        string aggregateType = "Payment",
        int eventVersion = 1) => new(
            Guid.NewGuid(),
            eventType,
            eventVersion,
            JsonSerializer.Serialize(payload),
            aggregateType,
            aggregateId,
            Guid.NewGuid(),
            UtcNow,
            UtcNow);

    private static void AssertDraft(
        NotificationDraft draft,
        Guid recipient,
        string type,
        string severity,
        string title,
        string body)
    {
        Assert.Equal(recipient, draft.RecipientUserId);
        Assert.Equal(type, draft.Type);
        Assert.Equal(severity, draft.Severity.ToString());
        Assert.Equal(title, draft.Title);
        Assert.Equal(body, draft.Body);
        Assert.Null(draft.ActionUrl);
    }

    private static void AssertFundedVariant(NotificationDraft draft)
    {
        Assert.Contains(
            draft.RecipientUserId,
            new[] { ClientUserId, LawyerUserId });
        Assert.Equal("milestone.funded", draft.Type);
        Assert.Equal("Success", draft.Severity.ToString());
        Assert.Equal("تم تمويل المرحلة", draft.Title);
        Assert.Null(draft.ActionUrl);
        AssertMilestoneData(draft.Data!);
    }

    private static void AssertMilestoneData(
        IReadOnlyDictionary<string, string> data)
    {
        Assert.Equal(MilestoneId.ToString(), data["milestoneId"]);
        Assert.Equal(ContractId.ToString(), data["contractId"]);
        Assert.Equal(ProposalId.ToString(), data["proposalId"]);
        Assert.Equal(LegalCaseId.ToString(), data["legalCaseId"]);
        Assert.Equal(4, data.Count);
    }

    private static void AssertSettlementData(
        IReadOnlyDictionary<string, string> data)
    {
        Assert.Equal(MilestoneId.ToString(), data["milestoneId"]);
        Assert.Equal(ContractId.ToString(), data["contractId"]);
        Assert.Equal(ProposalId.ToString(), data["proposalId"]);
        Assert.Equal(LegalCaseId.ToString(), data["legalCaseId"]);
        Assert.Equal(EscrowHoldId.ToString(), data["escrowHoldId"]);
        Assert.Equal(
            PaymentTransactionId.ToString(),
            data["paymentTransactionId"]);
        Assert.Equal(6, data.Count);
        Assert.DoesNotContain(
            data.Keys,
            key => key.Contains("amount", StringComparison.OrdinalIgnoreCase));
    }

    private sealed class StubMilestoneContextReader
        : IMilestoneNotificationContextReader
    {
        public Task<MilestoneNotificationContext> GetMilestoneAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(MilestoneId, milestoneId);
            return Task.FromResult(new MilestoneNotificationContext(
                MilestoneId,
                ContractId,
                ProposalId,
                LegalCaseId,
                ClientUserId,
                LawyerUserId));
        }

        public Task<MilestoneChangeRequestNotificationContext>
            GetChangeRequestAsync(
                Guid changeRequestId,
                CancellationToken cancellationToken) =>
            throw new NotSupportedException();
    }

    private sealed class StubPaymentContextReader(
        WithdrawalNotificationContext withdrawal)
        : IPaymentNotificationContextReader
    {
        public Task<WithdrawalNotificationContext> GetWithdrawalAsync(
            Guid withdrawalId,
            CancellationToken cancellationToken)
        {
            Assert.Equal(WithdrawalId, withdrawalId);
            return Task.FromResult(withdrawal);
        }

        public Task<WalletAdjustmentNotificationContext>
            GetWalletAdjustmentAsync(
                Guid walletAdjustmentId,
                CancellationToken cancellationToken)
        {
            Assert.Equal(AdjustmentId, walletAdjustmentId);
            return Task.FromResult(new WalletAdjustmentNotificationContext(
                AdjustmentId,
                LawyerUserId,
                ContractId));
        }
    }
}
