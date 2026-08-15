using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Exceptions;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.Payments.DTOs;
using SmartCourt.Infrastructure.Providers.Jobs;

namespace SmartCourt.Features.Payments;

[ApiController]
[Route("api/admin/milestones")]
[Authorize(Roles = "SuperAdministrator")]
public sealed class AdminEscrowReleaseController(
    IEscrowReleaseService escrowReleaseService)
    : ControllerBase
{
    [HttpPost("{milestoneId:guid}/release")]
    [SecurityRateLimit(RateLimitPolicyNames.AdminFinancialMutation)]
    public async Task<ActionResult<ApiResponse<PaymentActionResultDto>>>
        ForceReleaseAsync(
            Guid milestoneId,
            CancellationToken cancellationToken)
    {
        var result = await escrowReleaseService.ForceReleaseMilestoneAsync(
            milestoneId,
            cancellationToken);

        if (result.Outcome != JobExecutionOutcome.Completed)
        {
            throw new BusinessException(
                $"تعذر تحرير أموال المرحلة: {result.Reason}");
        }

        return Ok(ApiResponse<PaymentActionResultDto>.Ok(
            new PaymentActionResultDto(
                milestoneId,
                "Released",
                DateTimeOffset.UtcNow)));
    }
}