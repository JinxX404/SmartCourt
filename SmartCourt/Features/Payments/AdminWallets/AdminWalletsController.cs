using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/admin/wallets")]
[Authorize(Roles = "SuperAdministrator")]
public sealed class AdminWalletsController(
    IAdminWalletAdjustmentService adjustmentService,
    IValidator<AdminWalletAdjustmentRequest> adjustmentValidator)
    : ControllerBase
{
    [HttpPost("{lawyerUserId:guid}/adjustments")]
    [SecurityRateLimit(RateLimitPolicyNames.AdminFinancialMutation)]
    public async Task<ActionResult<ApiResponse<AdminWalletAdjustmentDto>>>
        AdjustAsync(
            Guid lawyerUserId,
            [FromBody] AdminWalletAdjustmentRequest request,
            [FromHeader(Name = "Idempotency-Key")]
            string? idempotencyKey,
            CancellationToken cancellationToken)
    {
        var validationResult = await adjustmentValidator.ValidateAsync(
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

        var result = await adjustmentService.AdjustAsync(
            lawyerUserId,
            request,
            idempotencyKey,
            cancellationToken);
        return Ok(ApiResponse<AdminWalletAdjustmentDto>.Ok(result));
    }
}

