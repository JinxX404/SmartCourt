using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/wallet")]
[Authorize(Roles = "Lawyer")]
[Produces("application/json")]
public sealed class WalletsController(
    IWalletService walletService,
    IValidator<CreateWithdrawalRequest> withdrawalValidator)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(ApiResponse<WalletDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<WalletDto>>> GetAsync(
        CancellationToken cancellationToken)
    {
        var wallet = await walletService.GetAsync(cancellationToken);
        return Ok(ApiResponse<WalletDto>.Ok(wallet));
    }

    [HttpPost("withdrawals")]
    [ProducesResponseType(
        typeof(ApiResponse<PaymentActionResultDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PaymentActionResultDto>>>
        WithdrawAsync(
            [FromBody] CreateWithdrawalRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        var validationResult = await withdrawalValidator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            throw new BusinessException(
                string.Join(
                    " ",
                    validationResult.Errors
                        .Select(error => error.ErrorMessage)
                        .Distinct(StringComparer.Ordinal)));
        }

        var result = await walletService.WithdrawAsync(
            request,
            idempotencyKey,
            cancellationToken);
        return Ok(ApiResponse<PaymentActionResultDto>.Ok(result));
    }
}
