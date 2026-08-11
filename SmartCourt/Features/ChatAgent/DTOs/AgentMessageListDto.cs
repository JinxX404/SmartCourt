namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record AgentMessageListDto(
    IReadOnlyList<AgentMessageDto> Items,
    bool HasMore);
