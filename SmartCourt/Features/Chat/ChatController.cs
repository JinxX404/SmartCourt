using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.GetConversation;
using SmartCourt.Features.Chat.GetConversations;
using SmartCourt.Features.Chat.GetMessages;
using SmartCourt.Features.Chat.SendMessage;

namespace SmartCourt.Features.Chat;

[ApiController]
[Authorize(Roles = "Client,Lawyer")]
[Route("api/chat")]
[Produces("application/json")]
public sealed class ChatController(IMediator mediator) : ControllerBase
{
    [HttpGet("conversations")]
    [ProducesResponseType(
        typeof(ApiResponse<ChatConversationPageDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChatConversationPageDto>>> ListAsync(
        [FromQuery] GetChatConversationsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(query, cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("conversations/{conversationId:guid}")]
    [ProducesResponseType(
        typeof(ApiResponse<ChatConversationDetailDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChatConversationDetailDto>>> GetAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetChatConversationQuery(conversationId),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpGet("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(
        typeof(ApiResponse<ChatMessagePageDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChatMessagePageDto>>> GetMessagesAsync(
        Guid conversationId,
        [FromQuery] int page,
        [FromQuery] int pageSize,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetChatMessagesQuery(
                conversationId,
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 50 : pageSize),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }

    [HttpPost("conversations/{conversationId:guid}/messages")]
    [ProducesResponseType(
        typeof(ApiResponse<ChatMessageDto>),
        StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessageAsync(
        Guid conversationId,
        [FromBody] SendChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new SendChatMessageCommand(conversationId, request.Content),
            cancellationToken);
        return StatusCode(result.StatusCode, result);
    }
}
