using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Domain;
using SmartCourt.Common.Models;
using SmartCourt.Features.LawyerSubscription.DTOs;
using SmartCourt.Features.LawyerSubscription.Enums;
using SmartCourt.Interfaces;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartCourt.Features.LawyerSubscription;

[ApiController]
[Route("api/lawyer")]
[Authorize(Roles = "Lawyer")]
public sealed class LawyerSubscriptionController : ControllerBase
{
    private readonly ILawyerQuotaService _quotaService;
    private readonly ILawyerSubscriptionPaymentService _paymentService;
    private readonly IOptions<LawyerPlanOptions> _planOptions;
    private readonly IOptions<List<TokenBundleOptions>> _bundleOptions;
    private readonly ICurrentUserService _currentUserService;

    public LawyerSubscriptionController(
        ILawyerQuotaService quotaService,
        ILawyerSubscriptionPaymentService paymentService,
        IOptions<LawyerPlanOptions> planOptions,
        IOptions<List<TokenBundleOptions>> bundleOptions,
        ICurrentUserService currentUserService)
    {
        _quotaService = quotaService;
        _paymentService = paymentService;
        _planOptions = planOptions;
        _bundleOptions = bundleOptions;
        _currentUserService = currentUserService;
    }

    [HttpGet("subscription")]
    public async Task<ActionResult<ApiResponse<LawyerQuotaInfoResponse>>> GetSubscriptionInfo(
        CancellationToken cancellationToken)
    {
        var lawyerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var quota = await _quotaService.GetQuotaAsync(lawyerId, cancellationToken);
        return Ok(ApiResponse<LawyerQuotaInfoResponse>.Ok(quota));
    }

    [HttpGet("subscription/plans")]
    public ActionResult<ApiResponse<LawyerPlanListDto>> GetPlans()
    {
        var plans = _planOptions.Value.Plans.Select(p => new LawyerPlanDto(
            p.PlanType,
            CreditConverter.ToCredits(p.DailyTokenLimit),
            p.MonthlyPriceEgp
        )).ToList();

        return Ok(ApiResponse<LawyerPlanListDto>.Ok(new LawyerPlanListDto(plans)));
    }

    [HttpPost("subscription/change")]
    public async Task<ActionResult<ApiResponse<LawyerPaymentCheckoutResponse>>> ChangePlan(
        [FromBody] ChangeLawyerPlanRequest request,
        [FromHeader(Name = "Payment-Method-Reference")] string? paymentMethodReference,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var confirmationReference = paymentMethodReference ?? string.Empty;
        if (!Enum.TryParse<LawyerPlanType>(request.PlanType, true, out var planType))
        {
            return BadRequest(ApiResponse<LawyerPaymentCheckoutResponse>.Fail("نوع الخطة غير صحيح."));
        }

        var response = await _paymentService.PurchaseSubscriptionAsync(planType, confirmationReference, idempotencyKey, cancellationToken);
        return Ok(ApiResponse<LawyerPaymentCheckoutResponse>.Ok(response));
    }

    [HttpGet("bundles")]
    public ActionResult<ApiResponse<LawyerBundleListDto>> GetBundles()
    {
        var bundles = _bundleOptions.Value.Select(b => new LawyerBundleDto(
            b.Id,
            b.Name,
            b.CreditAmount,
            b.PriceEgp
        )).ToList();

        return Ok(ApiResponse<LawyerBundleListDto>.Ok(new LawyerBundleListDto(bundles)));
    }

    [HttpPost("bundles/purchase")]
    public async Task<ActionResult<ApiResponse<LawyerPaymentCheckoutResponse>>> PurchaseBundle(
        [FromBody] LawyerBundlePurchaseRequest request,
        [FromHeader(Name = "Payment-Method-Reference")] string? paymentMethodReference,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var confirmationReference = paymentMethodReference ?? string.Empty;
        var response = await _paymentService.PurchaseBundleAsync(request.BundleId, confirmationReference, idempotencyKey, cancellationToken);
        return Ok(ApiResponse<LawyerPaymentCheckoutResponse>.Ok(response));
    }

    [HttpGet("quota/history")]
    public async Task<ActionResult<ApiResponse<LawyerQuotaHistoryResponse>>> GetQuotaHistory(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var lawyerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var history = await _quotaService.GetQuotaHistoryAsync(lawyerId, days, cancellationToken);
        return Ok(ApiResponse<LawyerQuotaHistoryResponse>.Ok(history));
    }

    [HttpGet("bundles/purchases")]
    public async Task<ActionResult<ApiResponse<LawyerQuotaTransactionListDto>>> GetBundlePurchases(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var lawyerId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        var purchases = await _quotaService.GetQuotaTransactionsAsync(lawyerId, page, pageSize, cancellationToken);
        return Ok(ApiResponse<LawyerQuotaTransactionListDto>.Ok(purchases));
    }
}
