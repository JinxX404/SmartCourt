using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCourt.Common.Configuration;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.ChatAgent.Monetization.DTOs;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.ChatAgent.Monetization;

[Route("api/chat-agent/bundles")]
[ApiController]
[Authorize(Roles = "Client")]
public class TokenBundlesController : ControllerBase
{
    private readonly ITokenBundlePurchaseService _purchaseService;
    private readonly IOptions<List<TokenBundleOptions>> _bundleOptions;

    private readonly ICurrentUserService _currentUserService;

    public TokenBundlesController(
        ITokenBundlePurchaseService purchaseService,
        IOptions<List<TokenBundleOptions>> bundleOptions,
        ICurrentUserService currentUserService)
    {
        _purchaseService = purchaseService;
        _bundleOptions = bundleOptions;
        _currentUserService = currentUserService;
    }

    [HttpGet("purchases")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    public async Task<ActionResult<ApiResponse<TokenBundlePurchaseListDto>>> GetPurchasesAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new Common.Exceptions.ForbiddenAccessException("يجب تسجيل الدخول.");
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var purchases = await _purchaseService.GetPurchasesAsync(currentUserId.Value, page, pageSize, cancellationToken);
        return ApiResponse<TokenBundlePurchaseListDto>.Ok(purchases);
    }

    [HttpGet]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    public ActionResult<ApiResponse<List<TokenBundleOptions>>> GetBundles()
    {
        var bundles = _bundleOptions.Value;
        return ApiResponse<List<TokenBundleOptions>>.Ok(bundles);
    }

    [HttpPost("purchase")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    public async Task<ActionResult<ApiResponse<TokenBundlePurchaseResponse>>> PurchaseBundle(
        [FromBody] TokenBundlePurchaseRequest request,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken cancellationToken)
    {
        var response = await _purchaseService.PurchaseBundleAsync(
            request.BundleId,
            request.ConfirmationTokenReference,
            idempotencyKey,
            cancellationToken);

        return ApiResponse<TokenBundlePurchaseResponse>.Created(response);
    }
}
