namespace SmartCourt.Features.Chat.DTOs;

public sealed record ChatParticipantDto(
    Guid UserId,
    string Name,
    string Role);

public sealed record ChatAttachmentDto(
    Guid Id,
    string FileName,
    string ContentType,
    long SizeInBytes,
    string DownloadUrl);

public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid? SenderUserId,
    string? SenderName,
    string Type,
    string Content,
    string? SystemCode,
    Guid? RelatedEntityId,
    DateTimeOffset CreatedAt,
    bool IsMine,
    IReadOnlyList<ChatAttachmentDto> Attachments);

public sealed record ChatConversationListItemDto(
    Guid Id,
    Guid ProposalId,
    Guid LegalCaseId,
    string CaseTitle,
    ChatParticipantDto Client,
    ChatParticipantDto Lawyer,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastMessageAt,
    ChatMessageDto? LastMessage)
{
    public bool CanSendMessages { get; init; }
    public bool CanUploadAttachments { get; init; }
}

public sealed record ChatConversationDetailDto(
    Guid Id,
    Guid ProposalId,
    Guid LegalCaseId,
    string CaseTitle,
    ChatParticipantDto Client,
    ChatParticipantDto Lawyer,
    string Status,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? LastMessageAt)
{
    public bool CanSendMessages { get; init; }
    public bool CanUploadAttachments { get; init; }
}

public sealed record ChatConversationPageDto(
    IReadOnlyList<ChatConversationListItemDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);

public sealed record ChatMessagePageDto(
    IReadOnlyList<ChatMessageDto> Items,
    int Page,
    int PageSize,
    int TotalCount,
    bool HasNextPage);
