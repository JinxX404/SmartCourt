using SmartCourt.Common.Domain;
using SmartCourt.Features.Files.Integration;

namespace SmartCourt.Features.Contracts.Entities;

public sealed class ContractFileAccessAudit
{
    private ContractFileAccessAudit()
    {
    }

    internal ContractFileAccessAudit(
        Guid id,
        Guid actorUserId,
        Guid storedFileId,
        ContractFilePurpose purpose,
        Guid relatedEntityId,
        bool moderatorAccess,
        DateTime accessedAt)
    {
        Id = EntityGuard.NotEmpty(id, nameof(id));
        ActorUserId = EntityGuard.NotEmpty(
            actorUserId,
            nameof(actorUserId));
        StoredFileId = EntityGuard.NotEmpty(
            storedFileId,
            nameof(storedFileId));
        Purpose = purpose;
        RelatedEntityId = EntityGuard.NotEmpty(
            relatedEntityId,
            nameof(relatedEntityId));
        ModeratorAccess = moderatorAccess;
        AccessedAt = EntityGuard.Utc(accessedAt, nameof(accessedAt));
    }

    public Guid Id { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public ContractFilePurpose Purpose { get; private set; }
    public Guid RelatedEntityId { get; private set; }
    public bool ModeratorAccess { get; private set; }
    public DateTime AccessedAt { get; private set; }
}
