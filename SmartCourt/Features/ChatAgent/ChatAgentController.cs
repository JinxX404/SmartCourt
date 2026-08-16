using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Common.RateLimiting;
using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent;

[ApiController]
[Authorize(Roles = "Client,Lawyer")]
[Route("api/agent")]
[Produces("application/json")]
public class ChatAgentController(IChatAgentService chatAgentService) : ControllerBase
{
    private readonly IChatAgentService _chatAgentService = chatAgentService;

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
        var result = await _chatAgentService.SendMessageAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AgentMessageDto>.Ok(result));
    }

    [AllowAnonymous]
    [HttpGet("conversations")]
    [SecurityRateLimit(RateLimitPolicyNames.AuthenticatedQuery)]
    [ProducesResponseType(typeof(ApiResponse<AgentConversationListDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<AgentConversationListDto>>> ListConversationsAsync(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _chatAgentService.ListConversationsAsync(page, pageSize, cancellationToken);
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
