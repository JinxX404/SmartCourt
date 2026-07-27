using Microsoft.EntityFrameworkCore;
using SmartCourt.Persistence;
using Xunit;

namespace SmartCourt.Tests.Persistence;

public sealed class ContractAndPaymentMigrationTests
{
    private const string ContractAndPaymentMigrationId =
        "20260727160911_ContractAndPaymentV1";

    [Fact]
    public void ContractAndPaymentMigration_IsDiscoverableByApplicationDbContext()
    {
        using var context = CreateContext();

        var migrations = context.Database.GetMigrations().ToArray();

        Assert.Contains(ContractAndPaymentMigrationId, migrations);
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

        Assert.True(baselineIndex >= 0);
        Assert.True(featureIndex > baselineIndex);
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
