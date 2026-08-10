namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record AgentConversationListDto(
    IReadOnlyList<AgentConversationDto> Items,
    int Page,
    int PageSize,
    int TotalCount);
