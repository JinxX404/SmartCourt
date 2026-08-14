using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
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
        Assert.Same(
            provider.GetRequiredService<IPaymentProvider>(),
            provider.GetRequiredService<IPaymentReconciliationProvider>());
        provider.GetRequiredService<IPaymentProviderStartupValidator>()
            .Validate();
        var options = provider
            .GetRequiredService<IOptions<PaymentProviderOptions>>()
            .Value;
        Assert.Equal("MockPaymentProvider", options.ProviderCode);
        Assert.Equal(65_536, options.WebhookMaximumBodySizeBytes);
        Assert.Empty(options.WebhookAllowedIpRanges);
        Assert.Equal(1_440, options.ProcessingSlaMinutes);
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
        Assert.Same(
            provider.GetRequiredService<IPaymentProvider>(),
            provider.GetRequiredService<IPaymentReconciliationProvider>());
        provider.GetRequiredService<IPaymentProviderStartupValidator>()
            .Validate();
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
    public void DisabledMockProvider_RequiresAnExplicitSupportedProvider()
    {
        Assert.Throws<InvalidOperationException>(() => BuildProvider(
            useMockProvider: false,
            isDevelopment: false));
    }

    [Fact]
    public void SplitOperationalAndReconciliationInstances_AreRejected()
    {
        var options = Options.Create(new PaymentProviderOptions
        {
            UseMockProvider = true
        });
        var operational = new MockPaymentProvider(
            options,
            NullLogger<MockPaymentProvider>.Instance);
        var reconciliation = new MockPaymentProvider(
            options,
            NullLogger<MockPaymentProvider>.Instance);
        var validator = new PaymentProviderStartupValidator(
            [operational],
            [reconciliation]);

        Assert.Throws<InvalidOperationException>(validator.Validate);
    }

    [Theory]
    [InlineData("PaymentProvider:ProviderCode", "")]
    [InlineData("PaymentProvider:WebhookMaximumBodySizeBytes", "1023")]
    [InlineData("PaymentProvider:WebhookMaximumBodySizeBytes", "1048577")]
    [InlineData("PaymentProvider:WebhookAllowedIpRanges:0", "invalid-range")]
    [InlineData("PaymentProvider:ProcessingSlaMinutes", "4")]
    [InlineData("PaymentProvider:ProcessingSlaMinutes", "10081")]
    public void InvalidWebhookSecurityConfiguration_FailsOptionsValidation(
        string key,
        string value)
    {
        using var provider = BuildProvider(
            useMockProvider: true,
            isDevelopment: true,
            new KeyValuePair<string, string?>(key, value));

        Assert.Throws<OptionsValidationException>(() =>
            _ = provider
                .GetRequiredService<IOptions<PaymentProviderOptions>>()
                .Value);
    }

    private static ServiceProvider BuildProvider(
        bool useMockProvider,
        bool isDevelopment,
        KeyValuePair<string, string?>? overrideSetting = null)
    {
        var settings = new Dictionary<string, string?>
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
        };
        if (overrideSetting.HasValue)
        {
            settings[overrideSetting.Value.Key] =
                overrideSetting.Value.Value;
        }

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructureServices(configuration, isDevelopment);
        return services.BuildServiceProvider();
    }
}
