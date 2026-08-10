using SmartCourt.Features.ChatAgent.DTOs;

namespace SmartCourt.Features.ChatAgent;

public interface IChatAgentService
{
    Task<AgentConversationDto> CreateConversationAsync(
        CreateAgentConversationRequest request,
        CancellationToken cancellationToken = default);

    Task<AgentConversationListDto> ListConversationsAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<AgentConversationDetailDto> GetConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task DeleteConversationAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);

    Task<AgentMessageDto> SendMessageAsync(
        Guid conversationId,
        SendAgentMessageRequest request,
        CancellationToken cancellationToken = default);

    Task<AgentMessageListDto> GetMessagesAsync(
        Guid conversationId,
        Guid? beforeMessageId,
        int limit,
        CancellationToken cancellationToken = default);

    Task<string?> GetOrFetchCaseContextAsync(
        Guid conversationId,
        CancellationToken cancellationToken = default);
}
