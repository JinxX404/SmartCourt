using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Admin.Quotas.DTOs;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Admin.Quotas;

[ApiController]
[Route("api/admin/quotas")]
[Authorize(Roles = "Admin")]
public class AdminQuotasController : ControllerBase
{
    private readonly IAdminQuotaService _adminQuotaService;
    private readonly ICurrentUserService _currentUserService;

    public AdminQuotasController(IAdminQuotaService adminQuotaService, ICurrentUserService currentUserService)
    {
        _adminQuotaService = adminQuotaService;
        _currentUserService = currentUserService;
    }

    [HttpPut("default-limit")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> SetGlobalDailyLimitAsync(
        [FromBody] UpdateDailyLimitRequest request,
        CancellationToken cancellationToken)
    {
        await _adminQuotaService.SetGlobalDailyLimitAsync(request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تحديث الحد اليومي الافتراضي بنجاح."));
    }

    [HttpPut("clients/{clientId:guid}/limit")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> SetClientDailyLimitAsync(
        [FromRoute] Guid clientId,
        [FromBody] UpdateDailyLimitRequest request,
        CancellationToken cancellationToken)
    {
        await _adminQuotaService.SetClientDailyLimitAsync(clientId, request, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تحديث الحد اليومي للعميل بنجاح."));
    }

    [HttpPost("clients/{clientId:guid}/adjust")]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<string>>> AdjustClientQuotaAsync(
        [FromRoute] Guid clientId,
        [FromBody] AdjustQuotaRequest request,
        CancellationToken cancellationToken)
    {
        var adminId = _currentUserService.UserId ?? Guid.Empty;
        await _adminQuotaService.AdjustClientQuotaAsync(clientId, request, adminId, cancellationToken);
        return Ok(ApiResponse<string>.Ok("تم تعديل رصيد العميل بنجاح."));
    }

    [HttpGet("clients/{clientId:guid}")]
    [ProducesResponseType(typeof(ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaInfoResponse>>> GetClientQuotaAsync(
        [FromRoute] Guid clientId,
        CancellationToken cancellationToken)
    {
        var result = await _adminQuotaService.GetClientQuotaAsync(clientId, cancellationToken);
        return Ok(ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaInfoResponse>.Ok(result));
    }

    [HttpGet("clients/{clientId:guid}/transactions")]
    [ProducesResponseType(typeof(ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaTransactionListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaTransactionListDto>>> GetClientQuotaTransactionsAsync(
        [FromRoute] Guid clientId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _adminQuotaService.GetClientQuotaTransactionsAsync(clientId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<SmartCourt.Features.ChatAgent.DTOs.QuotaTransactionListDto>.Ok(result));
    }

    [HttpGet("purchases")]
    [ProducesResponseType(typeof(ApiResponse<SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto>>> GetPurchasesAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _adminQuotaService.GetPurchasesAsync(page, pageSize, cancellationToken);
        return Ok(ApiResponse<SmartCourt.Features.ChatAgent.Monetization.DTOs.TokenBundlePurchaseListDto>.Ok(result));
    }

    [HttpGet("clients")]
    [ProducesResponseType(typeof(ApiResponse<AdminQuotaClientSummaryListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AdminQuotaClientSummaryListDto>>> GetClientsQuotaSummaryAsync(
        [FromQuery] string? search,
        [FromQuery] bool? isExhausted,
        [FromQuery] bool? hasAdditionalBalance,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var result = await _adminQuotaService.GetClientsQuotaSummaryAsync(search, isExhausted, hasAdditionalBalance, page, pageSize, cancellationToken);
        return Ok(ApiResponse<AdminQuotaClientSummaryListDto>.Ok(result));
    }

    [HttpGet("default-limit")]
    [ProducesResponseType(typeof(ApiResponse<GlobalDailyLimitResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<GlobalDailyLimitResponse>>> GetGlobalDailyLimitAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _adminQuotaService.GetGlobalDailyLimitAsync(cancellationToken);
        return Ok(ApiResponse<GlobalDailyLimitResponse>.Ok(result));
    }
}
