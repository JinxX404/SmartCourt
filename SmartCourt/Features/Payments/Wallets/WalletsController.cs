using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Common.Validation;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/wallet")]
[Authorize(Roles = "Lawyer")]
public sealed class WalletsController(
    IWalletService walletService,
    IValidator<CreateWithdrawalRequest> withdrawalValidator)
    : ControllerBase
{
    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    public async Task<ActionResult<ApiResponse<WalletDto>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var wallet = await walletService.GetAsync(cancellationToken);
        return Ok(ApiResponse<WalletDto>.Ok(wallet));
    }

    [HttpGet("withdrawals")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialQuery)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<WithdrawalDto>>>>
        GetWithdrawalsAsync(CancellationToken cancellationToken)
    {
        var withdrawals = await walletService.GetWithdrawalsAsync(
            cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<WithdrawalDto>>.Ok(withdrawals));
    }

    [HttpPost("withdrawals")]
    [SecurityRateLimit(RateLimitPolicyNames.FinancialMutation)]
    public async Task<ActionResult<ApiResponse<PaymentActionResultDto>>>
        WithdrawAsync(
            [FromBody] CreateWithdrawalRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        await withdrawalValidator.ValidateAndThrowBusinessExceptionAsync(
            request,
            cancellationToken);

        var result = await walletService.WithdrawAsync(
            request,
            idempotencyKey,
            cancellationToken);
        return Ok(ApiResponse<PaymentActionResultDto>.Ok(result));
    }
}


