using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/payment-methods")]
[Authorize(Roles = "Client")]
public sealed class ClientPaymentMethodsController(
    IClientPaymentMethodService service) : ControllerBase
{
    [HttpPost("setup-session")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<SetupPaymentMethodSessionDto>>>
        CreateSetupSessionAsync(
            [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
            CancellationToken cancellationToken)
        => Ok(ApiResponse<SetupPaymentMethodSessionDto>.Ok(
            await service.CreateSetupSessionAsync(
                idempotencyKey,
                cancellationToken)));

    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SavedPaymentMethodDto>>>>
        ListAsync(CancellationToken cancellationToken)
        => Ok(ApiResponse<IReadOnlyList<SavedPaymentMethodDto>>.Ok(
            await service.ListAsync(cancellationToken)));

    [HttpPut("{paymentMethodReference}/default")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<string>>> SetDefaultAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken)
    {
        await service.SetDefaultAsync(paymentMethodReference, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Default payment method updated."));
    }

    [HttpDelete("{paymentMethodReference}")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<string>>> RemoveAsync(
        string paymentMethodReference,
        CancellationToken cancellationToken)
    {
        await service.RemoveAsync(paymentMethodReference, cancellationToken);
        return Ok(ApiResponse<string>.Ok("Payment method removed."));
    }
}
