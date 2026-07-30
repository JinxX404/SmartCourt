using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Integration;
using SmartCourt.Infrastructure.Providers.Payments;
using SmartCourt.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Infrastructure.Providers;

public sealed class PaymentProviderRegistrationTests
{
    [Fact]
    public void DevelopmentConfiguration_RegistersMockProvider()
    {
        using var provider = BuildProvider(
            useMockProvider: true,
            isDevelopment: true);

        Assert.IsType<MockPaymentProvider>(
            provider.GetRequiredService<IPaymentProvider>());
    }

    [Fact]
    public void ProductionConfiguration_RegistersExplicitlyEnabledMockProvider()
    {
        using var provider = BuildProvider(
            useMockProvider: true,
            isDevelopment: false);

        var options = provider
            .GetRequiredService<IOptions<PaymentProviderOptions>>()
            .Value;

        Assert.True(options.UseMockProvider);
        Assert.IsType<MockPaymentProvider>(
            provider.GetRequiredService<IPaymentProvider>());
        Assert.IsType<ContractTerminationSettlementService>(
            provider.GetRequiredService<
                IContractTerminationSettlementService>());
        Assert.IsType<WalletService>(
            provider.GetRequiredService<IWalletService>());
        Assert.Contains(
            "not regulated escrow",
            options.Warning,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void DisabledMockProvider_IsNotSilentlyRegistered()
    {
        using var provider = BuildProvider(
            useMockProvider: false,
            isDevelopment: false);

        Assert.Null(provider.GetService<IPaymentProvider>());
    }

    private static ServiceProvider BuildProvider(
        bool useMockProvider,
        bool isDevelopment)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=localhost;Database=SmartCourtPaymentProviderTests;Trusted_Connection=True;TrustServerCertificate=True;",
                ["PaymentProvider:UseMockProvider"] =
                    useMockProvider.ToString(),
                ["PaymentProvider:Warning"] =
                    "The mock payment provider is not regulated escrow and is for tests only.",
                ["AuthEmail:PublicBaseUrl"] =
                    isDevelopment
                        ? "http://localhost:3000"
                        : "https://app.example.com",
                ["SmtpSettings:Server"] = "smtp.example.com",
                ["SmtpSettings:Port"] = "587",
                ["SmtpSettings:SenderName"] = "Smart Court",
                ["SmtpSettings:SenderEmail"] = "noreply@example.com",
                ["SmtpSettings:Username"] = "noreply@example.com",
                ["SmtpSettings:Password"] = "password",
                ["Jwt:Secret"] = "01234567890123456789012345678901",
                ["Jwt:Issuer"] = "SmartCourtAPI",
                ["Jwt:Audience"] = "SmartCourtClient"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructureServices(configuration, isDevelopment);
        return services.BuildServiceProvider();
    }
}
