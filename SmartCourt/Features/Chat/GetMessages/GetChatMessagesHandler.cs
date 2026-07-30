using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Interfaces;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.GetMessages;

public sealed class GetChatMessagesHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IValidator<GetChatMessagesQuery> validator)
    : IRequestHandler<GetChatMessagesQuery, ApiResponse<ChatMessagePageDto>>
{
    public async Task<ApiResponse<ChatMessagePageDto>> Handle(
        GetChatMessagesQuery request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ChatMessagePageDto>.Fail(
                validationResult.Errors.Select(error => error.ErrorMessage).ToList());
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        if (!await ChatAccess.IsParticipantAsync(
                context,
                request.ConversationId,
                actorUserId,
                cancellationToken))
        {
            return ApiResponse<ChatMessagePageDto>.Fail(
                "Conversation was not found.",
                404);
        }

        var query =
            from message in context.ChatMessages.AsNoTracking()
            join sender in context.Users.AsNoTracking()
                on message.SenderUserId equals sender.Id into senderJoin
            from sender in senderJoin.DefaultIfEmpty()
            where message.ConversationId == request.ConversationId
            select new
            {
                Message = message,
                SenderName = sender == null ? null : sender.FullName
            };

        var totalCount = await query.CountAsync(cancellationToken);
        var rows = await query
            .OrderByDescending(item => item.Message.CreatedAt)
            .ThenByDescending(item => item.Message.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        var items = rows
            .OrderBy(item => item.Message.CreatedAt)
            .ThenBy(item => item.Message.Id)
            .Select(item => new ChatMessageDto(
                item.Message.Id,
                item.Message.ConversationId,
                item.Message.SenderUserId,
                item.SenderName,
                item.Message.Type.ToString(),
                item.Message.Content,
                item.Message.SystemCode,
                item.Message.RelatedEntityId,
                item.Message.CreatedAt,
                item.Message.SenderUserId == actorUserId))
            .ToList();

        var page = new ChatMessagePageDto(
            items,
            request.Page,
            request.PageSize,
            totalCount,
            request.Page * request.PageSize < totalCount);
        return ApiResponse<ChatMessagePageDto>.Ok(page);
    }
}
