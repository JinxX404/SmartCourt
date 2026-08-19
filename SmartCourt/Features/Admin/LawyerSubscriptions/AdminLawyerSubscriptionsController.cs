using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.LawyerSubscriptions.DTOs;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Admin.LawyerSubscriptions;

[ApiController]
[Route("api/admin/lawyers")]
[Authorize(Roles = "Admin")]
public sealed class AdminLawyerSubscriptionsController : ControllerBase
{
    private readonly IAdminLawyerSubscriptionService _adminService;
    private readonly ICurrentUserService _currentUserService;

    public AdminLawyerSubscriptionsController(
        IAdminLawyerSubscriptionService adminService,
        ICurrentUserService currentUserService)
    {
        _adminService = adminService;
        _currentUserService = currentUserService;
    }

    [HttpGet("subscriptions")]
    [ProducesResponseType(typeof(ApiResponse<AdminLawyerSubscriptionListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminLawyerSubscriptionListDto>>> GetLawyersSubscriptionSummaryAsync(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _adminService.GetLawyersSubscriptionSummaryAsync(search, page, pageSize, cancellationToken);
        return Ok(ApiResponse<AdminLawyerSubscriptionListDto>.Ok(result));
    }

    [HttpGet("{lawyerId:guid}/subscription")]
    [ProducesResponseType(typeof(ApiResponse<LawyerQuotaInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LawyerQuotaInfoResponse>>> GetLawyerQuotaAsync(
        [FromRoute] Guid lawyerId,
        CancellationToken cancellationToken)
    {
        var result = await _adminService.GetLawyerQuotaAsync(lawyerId, cancellationToken);
        return Ok(ApiResponse<LawyerQuotaInfoResponse>.Ok(result));
    }

    [HttpGet("{lawyerId:guid}/subscription/transactions")]
    [ProducesResponseType(typeof(ApiResponse<LawyerQuotaTransactionListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<LawyerQuotaTransactionListDto>>> GetLawyerQuotaTransactionsAsync(
        [FromRoute] Guid lawyerId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _adminService.GetLawyerQuotaTransactionsAsync(lawyerId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<LawyerQuotaTransactionListDto>.Ok(result));
    }

    [HttpPost("{lawyerId:guid}/subscription/adjust-tokens")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> AdjustLawyerQuotaAsync(
        [FromRoute] Guid lawyerId,
        [FromBody] AdminAdjustLawyerTokensRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId ?? Guid.Empty;
        await _adminService.AdjustLawyerQuotaAsync(lawyerId, request, adminId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تعديل رصيد المحامي بنجاح."));
    }

    [HttpPut("{lawyerId:guid}/subscription/change-plan")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> ChangeLawyerPlanAsync(
        [FromRoute] Guid lawyerId,
        [FromBody] AdminChangeLawyerPlanRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId ?? Guid.Empty;
        await _adminService.ChangeLawyerPlanAsync(lawyerId, request, adminId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تغيير خطة المحامي بنجاح."));
    }
}
