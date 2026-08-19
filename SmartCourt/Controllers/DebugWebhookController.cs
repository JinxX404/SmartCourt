using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Features.Payments;
using SmartCourt.Features.Payments.Entities;
using SmartCourt.Providers.Payments;
using SmartCourt.Infrastructure.Providers.Payments;

namespace SmartCourt.Controllers;

#if DEBUG
[ApiController]
[Route("api/debug/webhooks")]
[AllowAnonymous]
public class DebugWebhookController(PaymentProviderWebhookService webhookService) : ControllerBase
{
    [HttpPost("simulate")]
    public async Task<IActionResult> SimulateWebhook(
        [FromQuery] string providerTransactionId, 
        [FromQuery] string eventType = "charge.succeeded")
    {
        var mockEvent = new ProviderWebhookEvent(
            EventId: "evt_debug_" + Guid.NewGuid().ToString("N"),
            EventType: eventType,
            ProviderObjectId: providerTransactionId,
            ConnectedAccountId: null,
            IsLive: false
        );

        var result = await webhookService.HandleAsync(mockEvent, CancellationToken.None);
        return Ok(result);
    }
}
#endif
