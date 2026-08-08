using SmartCourt.Common.Domain;
using SmartCourt.Features.Proposals.Entities;
using CaseEntity = SmartCourt.Entities.Case;

namespace SmartCourt.Features.Chat.Entities;

public sealed class ChatConversation
{
    private ChatConversation()
    {
    }

    internal ChatConversation(
        Guid id,
        Guid proposalId,
        Guid legalCaseId,
        Guid clientUserId,
        Guid lawyerUserId,
        DateTime createdAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ProposalId = EntityGuard.NotEmpty(proposalId, nameof(proposalId));
        LegalCaseId = EntityGuard.NotEmpty(legalCaseId, nameof(legalCaseId));
        ClientUserId = EntityGuard.NotEmpty(clientUserId, nameof(clientUserId));
        LawyerUserId = EntityGuard.NotEmpty(lawyerUserId, nameof(lawyerUserId));
        CreatedAt = EntityGuard.Utc(createdAt, nameof(createdAt));
        UpdatedAt = CreatedAt;
    }

    public Guid Id { get; internal set; }
    public Guid ProposalId { get; internal set; }
    public Guid LegalCaseId { get; internal set; }
    public Guid ClientUserId { get; internal set; }
    public Guid LawyerUserId { get; internal set; }
    public DateTime CreatedAt { get; internal set; }
    public DateTime UpdatedAt { get; internal set; }
    public DateTime? LastMessageAt { get; internal set; }
    public bool IsClosed { get; internal set; }
    public Proposal Proposal { get; internal set; } = null!;
    public CaseEntity Case { get; internal set; } = null!;
    public ICollection<ChatMessage> Messages { get; internal set; } = [];

    public bool HasParticipant(Guid userId)
    {
        return userId != Guid.Empty
            && (ClientUserId == userId || LawyerUserId == userId);
    }

    internal void MarkMessageAdded(DateTime sentAt)
    {
        LastMessageAt = EntityGuard.Utc(sentAt, nameof(sentAt));
        UpdatedAt = LastMessageAt.Value;
    }

    internal void Close(DateTime closedAt)
    {
        IsClosed = true;
        UpdatedAt = EntityGuard.Utc(closedAt, nameof(closedAt));
    }
}
