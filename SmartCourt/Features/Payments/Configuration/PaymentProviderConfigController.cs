using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/payments/config")]
[AllowAnonymous]
public sealed class PaymentProviderConfigController(
    IEnumerable<IPaymentBrowserConfigurationProvider> configurationProviders)
    : ControllerBase
{
    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    public ActionResult<ApiResponse<PaymentProviderConfigDto>> Get()
    {
        var configuration = configurationProviders.SingleOrDefault();
        if (configuration is null)
        {
            return Ok(ApiResponse<PaymentProviderConfigDto>.Ok(new(
                string.Empty,
                string.Empty,
                "EGP",
                false,
                false,
                false)));
        }

        var browser = configuration.BrowserConfiguration;
        return Ok(ApiResponse<PaymentProviderConfigDto>.Ok(new(
            browser.ProviderCode,
            browser.PublishableKey,
            browser.Currency,
            browser.SandboxOnly,
            browser.ConfirmationTokensEnabled,
            browser.SavedPaymentMethodsEnabled)));
    }
}
