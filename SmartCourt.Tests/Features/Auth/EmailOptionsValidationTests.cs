using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SmartCourt.Features.Auth.Shared;
using SmartCourt.Providers.Email;
using Xunit;

namespace SmartCourt.Tests.Features.Auth;

public sealed class EmailOptionsValidationTests
{
    [Fact]
    public void ProductionAcceptsHttpUrl_InTestingMode()
    {
        using var serviceProvider = BuildServiceProvider(
            publicBaseUrl: "http://localhost:3000",
            isDevelopment: false);

        var exception = Record.Exception(() =>
            serviceProvider.GetRequiredService<IOptions<AuthEmailOptions>>().Value);
        Assert.Null(exception);
    }

    [Fact]
    public void DevelopmentAcceptsConfiguredLocalHttpUrl()
    {
        using var serviceProvider = BuildServiceProvider(
            publicBaseUrl: "http://localhost:3000",
            isDevelopment: true);

        var options = serviceProvider
            .GetRequiredService<IOptions<AuthEmailOptions>>()
            .Value;

        Assert.Equal("http://localhost:3000", options.PublicBaseUrl);
    }

    [Fact]
    public void IncompleteSmtpSettingsDoNotThrow_InTestingMode()
    {
        using var serviceProvider = BuildServiceProvider(
            publicBaseUrl: "https://app.example.com",
            isDevelopment: false,
            smtpPassword: string.Empty);

        var exception = Record.Exception(() =>
            serviceProvider.GetRequiredService<IOptions<MailKitOptions>>().Value);
        Assert.Null(exception);
    }

    private static ServiceProvider BuildServiceProvider(
        string publicBaseUrl,
        bool isDevelopment,
        string smtpPassword = "password")
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] =
                    "Server=(localdb)\\MSSQLLocalDB;Database=SmartCourtOptionsTests;Trusted_Connection=True;",
                ["AuthEmail:PublicBaseUrl"] = publicBaseUrl,
                ["SmtpSettings:Server"] = "smtp.example.com",
                ["SmtpSettings:Port"] = "587",
                ["SmtpSettings:SenderName"] = "Smart Court",
                ["SmtpSettings:SenderEmail"] = "noreply@example.com",
                ["SmtpSettings:Username"] = "noreply@example.com",
                ["SmtpSettings:Password"] = smtpPassword,
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
