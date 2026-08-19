using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartCourt.Common.Models;
using SmartCourt.Entities;
using SmartCourt.Features.Chat.DTOs;
using SmartCourt.Features.Chat.Entities;
using SmartCourt.Features.Chat.Realtime;
using SmartCourt.Features.Chat.Shared;
using SmartCourt.Features.Contracts.Enums;
using SmartCourt.Features.Proposals.Enums;
using SmartCourt.Interfaces;
using SmartCourt.Interfaces.Providers;
using SmartCourt.Persistence;

namespace SmartCourt.Features.Chat.Attachments;

public sealed class SendChatAttachmentsHandler(
    ApplicationDbContext context,
    ICurrentUserService currentUserService,
    IValidator<SendChatAttachmentsCommand> validator,
    IFileStorageService fileStorageService,
    IChatRealtimeNotifier realtimeNotifier,
    TimeProvider timeProvider)
    : IRequestHandler<SendChatAttachmentsCommand, ApiResponse<ChatMessageDto>>
{
    public async Task<ApiResponse<ChatMessageDto>> Handle(
        SendChatAttachmentsCommand request,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(
            request,
            cancellationToken);
        if (!validationResult.IsValid)
        {
            return ApiResponse<ChatMessageDto>.Fail(
                validationResult.Errors
                    .Select(error => error.ErrorMessage)
                    .Distinct()
                    .ToList());
        }

        var actorUserId = ChatAccess.GetRequiredUserId(currentUserService);
        var conversation = await context.ChatConversations
            .Include(item => item.Proposal)
            .SingleOrDefaultAsync(
                item => item.Id == request.ConversationId,
                cancellationToken);
        if (conversation is null
            || !conversation.HasParticipant(actorUserId))
        {
            return ApiResponse<ChatMessageDto>.Fail(
                "Conversation was not found.",
                404);
        }

        var contractStatus = await context.Contracts
            .Where(contract => contract.ProposalId == conversation.ProposalId)
            .Select(contract => (ContractStatus?)contract.Status)
            .SingleOrDefaultAsync(cancellationToken);

        if (ChatAccess.IsHiddenFromLawyer(
                conversation.Proposal.Status,
                contractStatus,
                conversation.LawyerUserId,
                actorUserId))
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

        var inspectedFiles = new List<(IFormFile File, ChatAttachmentInspection Inspection)>();
        foreach (var file in request.Files)
        {
            var inspection = await ChatAttachmentPolicy.InspectAsync(
                file,
                cancellationToken);
            if (!inspection.IsValid)
            {
                return ApiResponse<ChatMessageDto>.Fail(
                    $"{Path.GetFileName(file.FileName)}: {inspection.Error}");
            }

            inspectedFiles.Add((file, inspection));
        }

        var now = timeProvider.GetUtcNow();
        var message = ChatMessage.CreateUserAttachmentMessage(
            Guid.NewGuid(),
            conversation.Id,
            actorUserId,
            request.Caption,
            inspectedFiles.Count,
            now);
        var uploadedPaths = new List<string>();

        try
        {
            foreach (var (file, inspection) in inspectedFiles)
            {
                var storedFileId = Guid.NewGuid();
                var storagePath = string.Join(
                    '/',
                    "chat-attachments",
                    conversation.LegalCaseId.ToString("N"),
                    conversation.Id.ToString("N"),
                    message.Id.ToString("N"),
                    $"{storedFileId:N}{inspection.Extension}");

                await using var stream = file.OpenReadStream();
                var upload = await fileStorageService.UploadAsync(
                    stream,
                    storagePath,
                    inspection.SafeFileName!,
                    inspection.ContentType,
                    cancellationToken);
                uploadedPaths.Add(upload.StoragePath);

                var storedFile = new StoredFile
                {
                    Id = storedFileId,
                    StoredFileName = Path.GetFileName(storagePath),
                    OriginalFileName = inspection.SafeFileName!,
                    FileUrl = upload.StoragePath,
                    ContentType = inspection.ContentType!,
                    Extension = inspection.Extension!,
                    SizeInBytes = upload.Size
                };
                var attachment = new ChatMessageAttachment(
                    Guid.NewGuid(),
                    message.Id,
                    storedFile.Id,
                    now)
                {
                    Message = message,
                    StoredFile = storedFile
                };

                context.StoredFiles.Add(storedFile);
                context.ChatMessageAttachments.Add(attachment);
            }

            context.ChatMessages.Add(message);
            conversation.MarkMessageAdded(now);
            await context.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            foreach (var storagePath in uploadedPaths)
            {
                try
                {
                    await fileStorageService.DeleteAsync(
                        storagePath,
                        CancellationToken.None);
                }
                catch
                {
                    // Storage cleanup is best effort; preserve the original failure.
                }
            }

            throw;
        }

        var dto = await ChatReadModel.FindMessageAsync(
            context,
            message.Id,
            actorUserId,
            cancellationToken);
        await realtimeNotifier.MessageCreatedAsync(dto!, cancellationToken);

        return ApiResponse<ChatMessageDto>.Created(dto!);
    }
}
