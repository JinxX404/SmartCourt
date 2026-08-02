using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Persistence;

public sealed class ContractAndPaymentMigrationTests
{
    private const string ContractAndPaymentMigrationId =
        "20260727160911_ContractAndPaymentV1";

    private const string ProposalWorkflowMigrationId =
        "20260730121329_AddProposalWorkflowFields";

    private const string PaymentReleaseRecoveryMigrationId =
        "20260802141816_AddPaymentReleaseRecovery";

    private const string CriticalFinancialUniquenessMigrationId =
        "20260802151827_EnforceCriticalFinancialUniqueness";

    private const string FinancialManualActionMigrationId =
        "20260802153701_AddFinancialManualActionEscalation";

    [Fact]
    public void ContractAndPaymentMigration_IsDiscoverableByApplicationDbContext()
    {
        using var context = CreateContext();

        var migrations = context.Database.GetMigrations().ToArray();

        Assert.Contains(ContractAndPaymentMigrationId, migrations);
        Assert.Contains(ProposalWorkflowMigrationId, migrations);
        Assert.Contains(PaymentReleaseRecoveryMigrationId, migrations);
        Assert.Contains(CriticalFinancialUniquenessMigrationId, migrations);
        Assert.Contains(FinancialManualActionMigrationId, migrations);
    }

    [Fact]
    public void ContractAndPaymentMigration_IsAfterTheExistingBaseline()
    {
        using var context = CreateContext();

        var migrations = context.Database.GetMigrations().ToArray();
        var baselineIndex = Array.IndexOf(
            migrations,
            "20260724174335_AddRowVersionToUserVerificationDocument");
        var featureIndex = Array.IndexOf(
            migrations,
            ContractAndPaymentMigrationId);
        var proposalWorkflowIndex = Array.IndexOf(
            migrations,
            ProposalWorkflowMigrationId);
        var paymentReleaseRecoveryIndex = Array.IndexOf(
            migrations,
            PaymentReleaseRecoveryMigrationId);
        var criticalFinancialUniquenessIndex = Array.IndexOf(
            migrations,
            CriticalFinancialUniquenessMigrationId);
        var financialManualActionIndex = Array.IndexOf(
            migrations,
            FinancialManualActionMigrationId);

        Assert.True(baselineIndex >= 0);
        Assert.True(featureIndex > baselineIndex);
        Assert.True(proposalWorkflowIndex > featureIndex);
        Assert.True(paymentReleaseRecoveryIndex > proposalWorkflowIndex);
        Assert.True(
            criticalFinancialUniquenessIndex
                > paymentReleaseRecoveryIndex);
        Assert.True(
            financialManualActionIndex
                > criticalFinancialUniquenessIndex);
    }

    [Fact]
    public void FinancialReconciliationQueueIndexes_AreDeployedByMigration()
    {
        using var context = CreateContext();
        var operations = GetCreateIndexOperations(context);

        var paymentQueue = operations.Last(item => item.Name
            == "IX_PaymentTransactions_ReconciliationQueue");
        Assert.False(paymentQueue.IsUnique);
        Assert.Equal("PaymentTransactions", paymentQueue.Table);
        Assert.Equal(
            new[] { "Status", "RequiresManualAction", "CreatedAt", "Id" },
            paymentQueue.Columns);

        var withdrawalQueue = operations.Last(item => item.Name
            == "IX_WithdrawalRequests_ReconciliationQueue");
        Assert.False(withdrawalQueue.IsUnique);
        Assert.Equal("WithdrawalRequests", withdrawalQueue.Table);
        Assert.Equal(
            new[] { "Status", "RequiresManualAction", "RequestedAt", "Id" },
            withdrawalQueue.Columns);
    }

    [Fact]
    public void CriticalFinancialUniqueIndexes_AreDeployedByMigrations()
    {
        using var context = CreateContext();
        var operations = GetCreateIndexOperations(context);

        AssertUniqueIndex(
            operations,
            "UX_PaymentWebhookEvents_EventId",
            "PaymentWebhookEvents",
            filter: null,
            "EventId");
        AssertUniqueIndex(
            operations,
            "UX_EscrowHolds_MilestoneId",
            "EscrowHolds",
            filter: null,
            "MilestoneId");
        AssertUniqueIndex(
            operations,
            "UX_Disputes_OpenPerMilestone",
            "Disputes",
            "[Status] IN (0, 1, 2)",
            "MilestoneId");
        AssertUniqueIndex(
            operations,
            "UX_PaymentTransactions_IdempotencyKey",
            "PaymentTransactions",
            filter: null,
            "IdempotencyKey");
        AssertUniqueIndex(
            operations,
            "UX_PaymentTransactions_ProviderTransaction",
            "PaymentTransactions",
            "[ProviderTransactionId] IS NOT NULL",
            "ProviderName",
            "ProviderTransactionId");
        AssertUniqueIndex(
            operations,
            "UX_WithdrawalRequests_IdempotencyKey",
            "WithdrawalRequests",
            filter: null,
            "IdempotencyKey");
        AssertUniqueIndex(
            operations,
            "UX_IdempotencyRecords_HoldSettlement",
            "IdempotencyRecords",
            "[ResourceType] = 'EscrowHoldSettlement'",
            "ResourceType",
            "ResourceId");
    }

    private static IReadOnlyList<CreateIndexOperation>
        GetCreateIndexOperations(ApplicationDbContext context)
    {
        var assembly = context.GetService<IMigrationsAssembly>();
        var providerName = context.Database.ProviderName!;
        return context.Database.GetMigrations()
            .Select(id => assembly.CreateMigration(
                assembly.Migrations[id],
                providerName))
            .SelectMany(migration => migration.UpOperations)
            .OfType<CreateIndexOperation>()
            .ToArray();
    }

    private static void AssertUniqueIndex(
        IReadOnlyList<CreateIndexOperation> operations,
        string name,
        string table,
        string? filter,
        params string[] columns)
    {
        var operation = operations.Last(item => item.Name == name);
        Assert.True(operation.IsUnique);
        Assert.Equal(table, operation.Table);
        Assert.Equal(columns, operation.Columns);
        Assert.Equal(filter, operation.Filter);
    }

    private static ApplicationDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                "Server=localhost;Database=SmartCourtMigrationMetadataTests;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;

        return new ApplicationDbContext(options);
    }
}
