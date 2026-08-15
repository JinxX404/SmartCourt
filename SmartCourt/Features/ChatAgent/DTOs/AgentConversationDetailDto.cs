namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record AgentConversationDetailDto(
    Guid Id,
    string? Title,
    Guid? CaseId,
    string? CaseTitle,
    string? CaseDescription,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);
