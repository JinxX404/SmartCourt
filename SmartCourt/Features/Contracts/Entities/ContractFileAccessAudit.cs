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
        string accessReason,
        bool moderatorAccess,
        DateTimeOffset accessedAt)
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
        AccessReason = accessReason;
        ModeratorAccess = moderatorAccess;
        AccessedAt = accessedAt;
    }

    public Guid Id { get; private set; }
    public Guid ActorUserId { get; private set; }
    public Guid StoredFileId { get; private set; }
    public ContractFilePurpose Purpose { get; private set; }
    public Guid RelatedEntityId { get; private set; }
    public string AccessReason { get; private set; }
    public bool ModeratorAccess { get; private set; }
    public DateTimeOffset AccessedAt { get; private set; }
}
