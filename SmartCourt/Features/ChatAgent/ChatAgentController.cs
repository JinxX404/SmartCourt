using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.ChatAgent.DTOs;

using SmartCourt.Interfaces;

namespace SmartCourt.Features.ChatAgent;

[ApiController]
[Authorize(Roles = "Client,Lawyer")]
[Route("api/agent")]
[Produces("application/json")]
public class ChatAgentController(
    IChatAgentService chatAgentService,
    IQuotaService quotaService,
    ICurrentUserService currentUserService) : ControllerBase
{
    private readonly IChatAgentService _chatAgentService = chatAgentService;
    private readonly IQuotaService _quotaService = quotaService;
    private readonly ICurrentUserService _currentUserService = currentUserService;

    [AllowAnonymous]
    [HttpPost("conversations")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [ProducesResponseType(typeof(ApiResponse<AgentConversationDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<AgentConversationDto>>> CreateConversationAsync(
        [FromBody] CreateAgentConversationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _chatAgentService.CreateConversationAsync(request, cancellationToken);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<AgentConversationDto>.Created(result));
    }

    [AllowAnonymous]
    [HttpPost("conversations/{id:guid}/messages")]
    [SecurityRateLimit(RateLimitPolicyNames.ChatAgentSend)]
    [ProducesResponseType(typeof(ApiResponse<AgentMessageDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentMessageDto>>> SendMessageAsync(
        [FromRoute] Guid id,
        [FromBody] SendAgentMessageRequest request,
        CancellationToken cancellationToken)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new Common.Exceptions.ForbiddenAccessException("يجب تسجيل الدخول لإرسال رسائل.");
        }

        var result = await _chatAgentService.SendMessageAsync(id, request, cancellationToken);
        
        bool isClient = User.IsInRole("Client");
        if (isClient)
        {
            var quota = await _quotaService.GetQuotaAsync(currentUserId.Value, cancellationToken);
            Response.Headers["X-RateLimit-Limit"] = quota.DailyCreditLimit.ToString();
            Response.Headers["X-RateLimit-Remaining"] = quota.TotalRemainingCredits.ToString();
        }

        return Ok(ApiResponse<AgentMessageDto>.Ok(result));
    }

    [HttpGet("quota")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<QuotaInfoResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotaInfoResponse>>> GetQuotaAsync(
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new Common.Exceptions.ForbiddenAccessException("يجب تسجيل الدخول.");
        }

        if (!User.IsInRole("Client"))
        {
            throw new Common.Exceptions.ForbiddenAccessException("خاصية الاستعلام عن الحصص متاحة للعملاء فقط.");
        }

        var quota = await _quotaService.GetQuotaAsync(currentUserId.Value, cancellationToken);
        return Ok(ApiResponse<QuotaInfoResponse>.Ok(quota));
    }

    [AllowAnonymous]
    [HttpGet("quota/default")]
    [ProducesResponseType(typeof(ApiResponse<DefaultQuotaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<DefaultQuotaResponse>>> GetDefaultQuotaAsync(CancellationToken cancellationToken = default)
    {
        var result = await _quotaService.GetDefaultQuotaAsync(cancellationToken);
        return Ok(ApiResponse<DefaultQuotaResponse>.Ok(result));
    }

    [HttpGet("quota/history")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<QuotaHistoryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotaHistoryResponse>>> GetQuotaHistoryAsync(
        [FromQuery] int days = 7,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new Common.Exceptions.ForbiddenAccessException("يجب تسجيل الدخول.");
        }

        if (!User.IsInRole("Client"))
        {
            throw new Common.Exceptions.ForbiddenAccessException("خاصية الاستعلام عن الحصص متاحة للعملاء فقط.");
        }

        // Limit the maximum days to prevent abuse
        days = Math.Clamp(days, 1, 30);

        var history = await _quotaService.GetQuotaHistoryAsync(currentUserId.Value, days, cancellationToken);
        return Ok(ApiResponse<QuotaHistoryResponse>.Ok(history));
    }

    [HttpGet("quota/transactions")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<QuotaTransactionListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<QuotaTransactionListDto>>> GetQuotaTransactionsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var currentUserId = _currentUserService.UserId;
        if (currentUserId == null)
        {
            throw new Common.Exceptions.ForbiddenAccessException("يجب تسجيل الدخول.");
        }

        if (!User.IsInRole("Client"))
        {
            throw new Common.Exceptions.ForbiddenAccessException("خاصية الاستعلام عن الحصص متاحة للعملاء فقط.");
        }

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var transactions = await _quotaService.GetQuotaTransactionsAsync(currentUserId.Value, page, pageSize, cancellationToken);
        return Ok(ApiResponse<QuotaTransactionListDto>.Ok(transactions));
    }

    [AllowAnonymous]
    [HttpGet("conversations")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<AgentConversationListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentConversationListDto>>> ListConversationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        var result = await _chatAgentService.ListConversationsAsync(page, pageSize, search, cancellationToken);
        return Ok(ApiResponse<AgentConversationListDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("conversations/{id:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<AgentConversationDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentConversationDetailDto>>> GetConversationAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _chatAgentService.GetConversationAsync(id, cancellationToken);
        return Ok(ApiResponse<AgentConversationDetailDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpPut("conversations/{id:guid}/title")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [ProducesResponseType(typeof(ApiResponse<AgentConversationDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentConversationDto>>> UpdateConversationTitleAsync(
        [FromRoute] Guid id,
        [FromBody] UpdateAgentConversationTitleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _chatAgentService.UpdateConversationTitleAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AgentConversationDto>.Ok(result, "تم تحديث عنوان المحادثة بنجاح."));
    }

    [AllowAnonymous]
    [HttpDelete("conversations/{id:guid}")]
    [SecurityRateLimit(RateLimitPolicyNames.StandardMutation)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse>> DeleteConversationAsync(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await _chatAgentService.DeleteConversationAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("تم حذف المحادثة بنجاح."));
    }

    [AllowAnonymous]
    [HttpGet("conversations/{id:guid}/messages")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<AgentMessageListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentMessageListDto>>> GetMessagesAsync(
        [FromRoute] Guid id,
        [FromQuery] Guid? before = null,
        [FromQuery] int limit = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _chatAgentService.GetMessagesAsync(id, before, limit, cancellationToken);
        return Ok(ApiResponse<AgentMessageListDto>.Ok(result));
    }
}
