using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/wallet/payout-account")]
[Authorize(Roles = "Lawyer")]
public sealed class LawyerPayoutAccountsController(
    ILawyerPayoutAccountService payoutAccountService) : ControllerBase
{
    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    public async Task<ActionResult<ApiResponse<LawyerPayoutAccountDto?>>>
        GetAsync(CancellationToken cancellationToken)
        => Ok(ApiResponse<LawyerPayoutAccountDto?>.Ok(
            await payoutAccountService.GetAsync(cancellationToken)));

    [HttpPost("onboarding-link")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<PayoutAccountLinkDto>>>
        CreateOnboardingLinkAsync(CancellationToken cancellationToken)
        => Ok(ApiResponse<PayoutAccountLinkDto>.Ok(
            await payoutAccountService.CreateOnboardingLinkAsync(
                cancellationToken)));

    [HttpPost("dashboard-link")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<PayoutAccountLinkDto>>>
        CreateDashboardLinkAsync(CancellationToken cancellationToken)
        => Ok(ApiResponse<PayoutAccountLinkDto>.Ok(
            await payoutAccountService.CreateDashboardLinkAsync(
                cancellationToken)));
}

[ApiController]
[Route("api/admin/payment-providers/stripe/connected-accounts")]
[Authorize(Roles = "FinanceAdministrator,SuperAdministrator")]
public sealed class AdminStripeConnectedAccountsController(
    ILawyerPayoutAccountService payoutAccountService) : ControllerBase
{
    [HttpPost("link")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<LawyerPayoutAccountDto>>>
        LinkAsync(
            [FromBody] LinkLawyerPayoutAccountRequest request,
            CancellationToken cancellationToken)
        => Ok(ApiResponse<LawyerPayoutAccountDto>.Ok(
            await payoutAccountService.LinkSandboxAccountAsync(
                request,
                cancellationToken)));
}
