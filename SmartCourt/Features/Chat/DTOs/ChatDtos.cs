namespace SmartCourt.Features.Chat.DTOs;

public sealed record ChatParticipantDto(
    Guid UserId,
    string Name,
    string Role);

public sealed record ChatMessageDto(
    Guid Id,
    Guid ConversationId,
    Guid? SenderUserId,
    string? SenderName,
    string Type,
    string Content,
    string? SystemCode,
    Guid? RelatedEntityId,
    DateTime CreatedAt,
    bool IsMine);

public sealed record ChatConversationListItemDto(
    Guid Id,
    Guid ProposalId,
    Guid LegalCaseId,
    string CaseTitle,
    ChatParticipantDto Client,
    ChatParticipantDto Lawyer,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastMessageAt,
    ChatMessageDto? LastMessage);

public sealed record ChatConversationDetailDto(
    Guid Id,
    Guid ProposalId,
    Guid LegalCaseId,
    string CaseTitle,
    ChatParticipantDto Client,
    ChatParticipantDto Lawyer,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? LastMessageAt);

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
