namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record AgentConversationDto(
    Guid Id,
    string? Title,
    Guid? CaseId,
    string? CaseTitle,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
