namespace SmartCourt.Features.ChatAgent.DTOs;

public sealed record CreateAgentConversationRequest(
    Guid? CaseId = null);
