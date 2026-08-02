using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using SmartCourt.Features.Contracts.Entities;
using SmartCourt.Features.Disputes.Entities;
using SmartCourt.Features.Milestones.Entities;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Infrastructure.Persistence.Entities;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Persistence;

public sealed class ContractAndPaymentConfigurationTests
{
    [Fact]
    public void ApplicationDbContext_DiscoversEveryContractPaymentConfiguration()
    {
        using var context = CreateContext();
        var expectedTypes = new[]
        {
            typeof(Contract),
            typeof(Milestone),
            typeof(MilestoneChangeRequest),
            typeof(MilestoneSubmission),
            typeof(MilestoneSubmissionAttachment),
            typeof(ContractAttachment),
            typeof(EscrowAccount),
            typeof(EscrowHold),
            typeof(EscrowLedgerEntry),
            typeof(PaymentTransaction),
            typeof(PaymentWebhookEvent),
            typeof(LawyerWallet),
            typeof(WithdrawalRequest),
            typeof(Dispute),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory),
            typeof(IdempotencyRecord),
            typeof(OutboxMessage)
        };

        foreach (var type in expectedTypes)
        {
            Assert.NotNull(context.Model.FindEntityType(type));
        }
    }

    [Fact]
    public void MoneyProperties_UseDecimal18Scale2()
    {
        using var context = CreateContext();
        var moneyProperties = new[]
        {
            (typeof(Milestone), "Amount"),
            (typeof(EscrowAccount), "TotalDeposited"),
            (typeof(EscrowAccount), "TotalReleased"),
            (typeof(EscrowAccount), "TotalRefunded"),
            (typeof(EscrowAccount), "TotalFees"),
            (typeof(EscrowHold), "GrossAmount"),
            (typeof(EscrowHold), "PlatformFeeAmount"),
            (typeof(EscrowHold), "NetAmount"),
            (typeof(EscrowLedgerEntry), "Amount"),
            (typeof(EscrowLedgerEntry), "RunningBalance"),
            (typeof(PaymentTransaction), "Amount"),
            (typeof(LawyerWallet), "PendingBalance"),
            (typeof(LawyerWallet), "AvailableBalance"),
            (typeof(WithdrawalRequest), "Amount"),
            (typeof(Dispute), "ResolutionAmount"),
            (typeof(DisputeResolution), "GrossHoldAmount"),
            (typeof(DisputeResolution), "ClientRefundAmount"),
            (typeof(DisputeResolution), "LawyerReleaseAmount"),
            (typeof(DisputeResolution), "PlatformFeeAmount")
        };

        foreach (var (entityType, propertyName) in moneyProperties)
        {
            var property = context.Model
                .FindEntityType(entityType)!
                .FindProperty(propertyName);

            Assert.NotNull(property);
            Assert.Equal(18, property!.GetPrecision());
            Assert.Equal(2, property.GetScale());
            Assert.Equal("decimal(18,2)", property.GetColumnType());
        }
    }

    [Fact]
    public void MutableRoots_UseRowVersion_AndAppendOnlyRowsDoNot()
    {
        using var context = CreateContext();
        var mutableRoots = new[]
        {
            typeof(Contract),
            typeof(Milestone),
            typeof(MilestoneChangeRequest),
            typeof(EscrowAccount),
            typeof(EscrowHold),
            typeof(PaymentTransaction),
            typeof(LawyerWallet),
            typeof(WithdrawalRequest),
            typeof(Dispute),
            typeof(IdempotencyRecord),
            typeof(OutboxMessage)
        };
        var appendOnlyRows = new[]
        {
            typeof(MilestoneSubmission),
            typeof(MilestoneSubmissionAttachment),
            typeof(ContractAttachment),
            typeof(EscrowLedgerEntry),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory)
        };

        foreach (var type in mutableRoots)
        {
            var property = context.Model
                .FindEntityType(type)!
                .FindProperty("RowVersion");
            Assert.NotNull(property);
            Assert.True(property!.IsConcurrencyToken);
            Assert.Equal(
                ValueGenerated.OnAddOrUpdate,
                property.ValueGenerated);
        }

        foreach (var type in appendOnlyRows)
        {
            Assert.Null(context.Model.FindEntityType(type)!.FindProperty("RowVersion"));
        }
    }

    [Fact]
    public void RequiredUniqueIndexesAndChecks_ArePresent()
    {
        using var context = CreateContext();

        AssertUniqueIndex<Contract>(context, "ProposalId");
        AssertUniqueCompositeIndex<Milestone>(
            context,
            "ContractId",
            "OrderNumber");
        AssertUniqueIndex<EscrowAccount>(context, "ContractId");
        AssertUniqueIndex<EscrowHold>(context, "MilestoneId");
        AssertUniqueCompositeIndex<MilestoneSubmission>(
            context,
            "MilestoneId",
            "Version");
        AssertUniqueIndex<LawyerWallet>(context, "LawyerUserId");
        AssertUniqueIndex<DisputeResolution>(context, "DisputeId");

        AssertCheck<Contract>(context, "CK_Contracts_Currency_EGP");
        AssertCheck<Milestone>(context, "CK_Milestones_Amount_Positive");
        AssertCheck<EscrowHold>(context, "CK_EscrowHolds_Reconciliation");
        AssertCheck<PaymentTransaction>(
            context,
            "CK_PaymentTransactions_CompletedDepositRequiresHold");
        AssertCheck<PaymentTransaction>(
            context,
            "CK_PaymentTransactions_ProviderAttemptCount_NonNegative");
        AssertCheck<PaymentTransaction>(
            context,
            "CK_PaymentTransactions_ManualActionTimestamp");
        Assert.Contains(
            context.Model.FindEntityType(typeof(PaymentTransaction))!
                .GetIndexes(),
            index => index.GetDatabaseName()
                == "IX_PaymentTransactions_ReleaseRecovery");
        Assert.Contains(
            context.Model.FindEntityType(typeof(PaymentTransaction))!
                .GetIndexes(),
            index => index.GetDatabaseName()
                == "IX_PaymentTransactions_ReconciliationQueue");
        AssertCheck<WithdrawalRequest>(
            context,
            "CK_WithdrawalRequests_ManualActionTimestamp");
        Assert.Contains(
            context.Model.FindEntityType(typeof(WithdrawalRequest))!
                .GetIndexes(),
            index => index.GetDatabaseName()
                == "IX_WithdrawalRequests_ReconciliationQueue");
        AssertCheck<DisputeResolution>(
            context,
            "CK_DisputeResolutions_Reconciliation");
        AssertCheck<DisputeEvidence>(
            context,
            "CK_DisputeEvidence_FileOrContent");
    }

    [Fact]
    public void CriticalFinancialUniquenessRules_ArePreciselyConfigured()
    {
        using var context = CreateContext();

        AssertNamedUniqueIndex<PaymentWebhookEvent>(
            context,
            "UX_PaymentWebhookEvents_EventId",
            filter: null,
            "EventId");
        AssertNamedUniqueIndex<EscrowHold>(
            context,
            "UX_EscrowHolds_MilestoneId",
            filter: null,
            "MilestoneId");
        AssertNamedUniqueIndex<Dispute>(
            context,
            "UX_Disputes_OpenPerMilestone",
            "[Status] IN (0, 1, 2)",
            "MilestoneId");
        AssertNamedUniqueIndex<PaymentTransaction>(
            context,
            "UX_PaymentTransactions_IdempotencyKey",
            filter: null,
            "IdempotencyKey");
        AssertNamedUniqueIndex<PaymentTransaction>(
            context,
            "UX_PaymentTransactions_ProviderTransaction",
            "[ProviderTransactionId] IS NOT NULL",
            "ProviderName",
            "ProviderTransactionId");
        AssertNamedUniqueIndex<WithdrawalRequest>(
            context,
            "UX_WithdrawalRequests_IdempotencyKey",
            filter: null,
            "IdempotencyKey");
        AssertNamedUniqueIndex<IdempotencyRecord>(
            context,
            "UX_IdempotencyRecords_HoldSettlement",
            "[ResourceType] = 'EscrowHoldSettlement'",
            "ResourceType",
            "ResourceId");
    }

    [Fact]
    public void FinancialAndDisputeRelationships_RestrictDeletes()
    {
        using var context = CreateContext();
        var restrictedTypes = new[]
        {
            typeof(Milestone),
            typeof(EscrowAccount),
            typeof(EscrowHold),
            typeof(EscrowLedgerEntry),
            typeof(PaymentTransaction),
            typeof(LawyerWallet),
            typeof(WithdrawalRequest),
            typeof(Dispute),
            typeof(DisputeResolution),
            typeof(DisputeEvidence),
            typeof(LawyerPenalty),
            typeof(ContractStateHistory),
            typeof(MilestoneStateHistory)
        };

        foreach (var type in restrictedTypes)
        {
            var foreignKeys = context.Model
                .FindEntityType(type)!
                .GetForeignKeys();
            Assert.NotEmpty(foreignKeys);
            Assert.All(
                foreignKeys,
                foreignKey => Assert.Equal(
                    DeleteBehavior.Restrict,
                    foreignKey.DeleteBehavior));
        }

        var attachmentForeignKeys = context.Model
            .FindEntityType(typeof(MilestoneSubmissionAttachment))!
            .GetForeignKeys();
        Assert.Contains(
            attachmentForeignKeys,
            foreignKey => foreignKey.DeleteBehavior == DeleteBehavior.Cascade);
    }

    private static void AssertUniqueIndex<TEntity>(
        ApplicationDbContext context,
        string propertyName)
    {
        var entity = context.Model.FindEntityType(typeof(TEntity))!;
        Assert.Contains(
            entity.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Select(property => property.Name)
                    .SequenceEqual([propertyName]));
    }

    private static void AssertUniqueCompositeIndex<TEntity>(
        ApplicationDbContext context,
        params string[] propertyNames)
    {
        var entity = context.Model.FindEntityType(typeof(TEntity))!;
        Assert.Contains(
            entity.GetIndexes(),
            index => index.IsUnique
                && index.Properties.Select(property => property.Name)
                    .SequenceEqual(propertyNames));
    }

    private static void AssertCheck<TEntity>(
        ApplicationDbContext context,
        string checkName)
    {
        var entity = context.GetService<IDesignTimeModel>()
            .Model
            .FindEntityType(typeof(TEntity))!;
        Assert.Contains(entity.GetCheckConstraints(), check => check.Name == checkName);
    }

    private static void AssertNamedUniqueIndex<TEntity>(
        ApplicationDbContext context,
        string databaseName,
        string? filter,
        params string[] propertyNames)
    {
        var entity = context.Model.FindEntityType(typeof(TEntity))!;
        var index = Assert.Single(
            entity.GetIndexes(),
            item => item.GetDatabaseName() == databaseName);
        Assert.True(index.IsUnique);
        Assert.Equal(
            propertyNames,
            index.Properties.Select(property => property.Name));
        Assert.Equal(filter, index.GetFilter());
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=SmartCourtModelTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new ApplicationDbContext(options);
    }
}
