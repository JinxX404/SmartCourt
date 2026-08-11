using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.SendMessage;

public sealed class SendChatMessageHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IValidator<SendChatMessageCommand> validator,
    IChatRealtimeNotifier realtimeNotifier,
    TimeProvider timeProvider)
    : IRequestHandler<SendChatMessageCommand, ApiResponse<ChatMessageDto>>
{
    public async Task<ApiResponse<ChatMessageDto>> Handle(
        SendChatMessageCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ChatMessageDto>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        var conversation = await context.ChatConversations
            .Include(item => item.Proposal)
            .SingleOrDefaultAsync(
                item => item.Id == request.ConversationId,
                cancellationToken);
        if (conversation is null || !conversation.HasParticipant(actorUserId))
        {
            return ApiResponse<ChatMessageDto>.Fail(
                "Conversation was not found.",
                404);
        }

        if (conversation.Proposal.Status == ProposalStatus.Superseded
            && conversation.LawyerUserId == actorUserId)
        {
            return ApiResponse<ChatMessageDto>.Fail(
                "Conversation was not found.",
                404);
        }

        if (conversation.IsClosed
            || conversation.Proposal.Status != ProposalStatus.Accepted)
        {
            return ApiResponse<ChatMessageDto>.Fail(
                "Conversation is closed.",
                409);
        }

        var now = timeProvider.GetUtcNow().UtcDateTime;
        var message = ChatMessage.CreateUserMessage(
            Guid.NewGuid(),
            conversation.Id,
            actorUserId,
            request.Content,
            now);
        context.ChatMessages.Add(message);
        conversation.MarkMessageAdded(now);

        await context.SaveChangesAsync(cancellationToken);

        var dto = await ChatReadModel.FindMessageAsync(
            context,
            message.Id,
            actorUserId,
            cancellationToken);
        await realtimeNotifier.MessageCreatedAsync(
            dto!,
            cancellationToken);

        return ApiResponse<ChatMessageDto>.Ok(dto!);
    }
}
