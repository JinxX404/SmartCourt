namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record AgentMessageDto(
    Guid Id,
    string Role,
    string Content,
    DateTime CreatedAt);
