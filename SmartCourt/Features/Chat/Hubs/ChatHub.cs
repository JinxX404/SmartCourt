using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.SendMessage;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Interfaces;

namespace SmartCourt.Features.Chat.Hubs;

[Authorize(Roles = "Client,Lawyer")]
public sealed class ChatHub(
    IMediator mediator,
    IChatConversationService conversationService,
    ICurrentUserService currentUserService) : Hub<IChatClient>
{
    public async Task JoinConversation(Guid conversationId)
    {
        var cancellationToken = Context.ConnectionAborted;
        if (!await CanAccessConversationAsync(
                conversationId,
                cancellationToken))
        {
            throw new HubException("Conversation was not found.");
        }

        await Groups.AddToGroupAsync(
            Context.ConnectionId,
            ChatGroups.Conversation(conversationId),
            cancellationToken);
    }

    public async Task LeaveConversation(Guid conversationId)
    {
        await Groups.RemoveFromGroupAsync(
            Context.ConnectionId,
            ChatGroups.Conversation(conversationId),
            Context.ConnectionAborted);
    }

    public async Task<ChatMessageDto> SendMessage(
        Guid conversationId,
        SendChatMessageRequest request)
    {
        var result = await mediator.Send(
            new SendChatMessageCommand(conversationId, request.Content),
            Context.ConnectionAborted);
        if (!result.Success || result.Data is null)
        {
            throw new HubException(
                result.Message
                    ?? result.Errors?.FirstOrDefault()
                    ?? "Message could not be sent.");
        }

        return result.Data;
    }

    private async Task<bool> CanAccessConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId;
        if (!userId.HasValue)
        {
            return false;
        }

        return await conversationService.CanAccessConversationAsync(
            conversationId,
            userId.Value,
            cancellationToken);
    }
}
