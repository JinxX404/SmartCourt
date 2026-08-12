using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using SmartCourt.Common.Models;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Payments;
using Xunit;

namespace SmartCourt.Tests.Features.Payments;

public sealed class PaymentProviderConfigControllerTests
{
    [Fact]
    public void StripeConfig_ExposesOnlyBrowserSafeTestConfiguration()
    {
        var controller = new PaymentProviderConfigController(
            [new BrowserConfigurationStub()]);

        var action = controller.Get();
        var result = Assert.IsType<OkObjectResult>(
            ((IConvertToActionResult)action).Convert());
        var response = Assert.IsType<ApiResponse<PaymentProviderConfigDto>>(
            result.Value);

        Assert.Equal("pk_test_browser_safe", response.Data!.PublishableKey);
        Assert.True(response.Data.SandboxOnly);
        Assert.True(response.Data.ConfirmationTokensEnabled);
        Assert.DoesNotContain(
            "private_server_value",
            System.Text.Json.JsonSerializer.Serialize(response));
        Assert.Equal(StatusCodes.Status200OK, response.StatusCode);
    }

    private sealed class BrowserConfigurationStub
        : IPaymentBrowserConfigurationProvider
    {
        public ProviderBrowserConfiguration BrowserConfiguration => new(
            "StripeConnect",
            "pk_test_browser_safe",
            "EGP",
            SandboxOnly: true,
            IsTestEnvironment: true,
            ConfirmationTokensEnabled: true,
            SavedPaymentMethodsEnabled: true);
    }
}
